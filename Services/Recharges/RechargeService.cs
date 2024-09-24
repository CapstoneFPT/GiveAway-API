using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using BusinessObjects.Dtos.Commons;
using BusinessObjects.Dtos.Recharges;
using BusinessObjects.Entities;
using DotNext;
using LinqKit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Quartz;
using Repositories.Accounts;
using Repositories.Recharges;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Services.VnPayService;
using Services.Transactions;

namespace Services.Recharges;

public class RechargeService : IRechargeService
{
    private readonly IRechargeRepository _rechargeRepository;
    private readonly ILogger<RechargeService> _logger;
    private readonly ISchedulerFactory _schedulerFactory;
    private readonly IAccountRepository _accountRepository;
    private readonly IVnPayService _vnPayService;
    private readonly ITransactionService _transactionService;
    private readonly IConfiguration _configuration;

    public RechargeService(IRechargeRepository rechargeRepository, ILogger<RechargeService> logger,
        ISchedulerFactory schedulerFactory, IAccountRepository accountRepository, IVnPayService vnPayService, ITransactionService transactionService, IConfiguration configuration)

    {
        _rechargeRepository = rechargeRepository;
        _logger = logger;
        _schedulerFactory = schedulerFactory;
        _accountRepository = accountRepository;
        _vnPayService = vnPayService;
        _transactionService = transactionService;
        _configuration = configuration;
    }

    private Expression<Func<Recharge, bool>> GetPredicate(GetRechargesRequest request)
    {
        Expression<Func<Recharge, bool>> predicate = recharge => true;

        if (request.MemberId.HasValue)
        {
            predicate = predicate.And(recharge => recharge.MemberId == request.MemberId.Value);
        }

        if (request.RechargeStatus != null)
        {
            predicate = predicate.And(recharge => recharge.Status == request.RechargeStatus.Value);
        }

        if (request.RechargeCode != null)
        {
            predicate = predicate.And(recharge => EF.Functions.ILike(recharge.RechargeCode, $"%{request.RechargeCode}%"));
        }

        return predicate;
    }

    public async Task<DotNext.Result<PaginationResponse<RechargeListResponse>, ErrorCode>> GetRecharges(
        GetRechargesRequest request)
    {
        var query = _rechargeRepository.GetQueryable();
        Expression<Func<Recharge, bool>> predicate = GetPredicate(request);
        Expression<Func<Recharge, RechargeListResponse>> selector = recharge => new RechargeListResponse
        {
            RechargeId = recharge.RechargeId,
            MemberId = recharge.MemberId,
            Amount = recharge.Amount,
            Status = recharge.Status,
            RechargeCode = recharge.RechargeCode,
            CreatedDate = recharge.CreatedDate,
            PaymentMethod = recharge.PaymentMethod
        };

        var count = await query.Where(predicate).CountAsync();
        query = query.Where(predicate);

        if (request.Page != null && request.PageSize != null && request.PageSize.Value > 0 && request.Page.Value > 0)
        {
            query = query.Skip((request.Page.Value - 1) * request.PageSize.Value).Take(request.PageSize.Value);
        }
        
        var result = await query.Select(selector).ToListAsync();

        return new PaginationResponse<RechargeListResponse>()
        {
            PageSize = request.PageSize ?? -1,
            PageNumber = request.Page ?? -1,
            TotalCount = count,
            Items = result
        };
    }

    public async Task<Result<Recharge, ErrorCode>> CreateRecharge(Recharge recharge)
    {
        try
        {
            var result = await _rechargeRepository.CreateRecharge(recharge);

            if (result == null)
            {
                return new Result<Recharge, ErrorCode>(ErrorCode.ServerError);
            }

            await ScheduleRechargeExpiration(result);

            return new Result<Recharge, ErrorCode>(result);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error creating recharge {RechargeId}", recharge.RechargeId);
            return new Result<Recharge, ErrorCode>(ErrorCode.ServerError);
        }
    }

    private async Task ScheduleRechargeExpiration(Recharge recharge)
    {
        var scheduler = await _schedulerFactory.GetScheduler();
        var jobDataMap = new JobDataMap()
        {
            { "RechargeId", recharge.RechargeId }
        };

        var job = JobBuilder.Create<RechargeExpirationJob>()
            .WithIdentity($"RechargeExpirationJob-{recharge.RechargeId}")
            .UsingJobData(jobDataMap)
            .Build();

        var trigger = TriggerBuilder.Create()
            .WithIdentity($"RechargeExpirationTrigger-{recharge.RechargeId}")
            .StartAt(DateBuilder.FutureDate(15, IntervalUnit.Minute))
            .Build();
        await scheduler.ScheduleJob(job, trigger);
    }

    public async Task<Result<Recharge, ErrorCode>> GetRechargeById(Guid rechargeId)
    {
        try
        {
            var recharge = await _rechargeRepository.GetRechargeById(rechargeId);
            if (recharge == null)
            {
                return new Result<Recharge, ErrorCode>(ErrorCode.NotFound);
            }

            return new Result<Recharge, ErrorCode>(recharge);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error getting recharge {RechargeId}", rechargeId);
            return new Result<Recharge, ErrorCode>(ErrorCode.ServerError);
        }
    }

    public async Task<Result<bool, ErrorCode>> CompleteRecharge(Guid rechargeId, decimal amount)
    {
        try
        {
            var rechargeResult = await GetRechargeById(rechargeId);
            if (!rechargeResult.IsSuccessful)
            {
                return new Result<bool, ErrorCode>(rechargeResult.Error);
            }

            var recharge = rechargeResult.Value;

            if (recharge.Status != RechargeStatus.Pending)
            {
                return new Result<bool, ErrorCode>(ErrorCode.InvalidOperation);
            }

            recharge.Status = RechargeStatus.Completed;
            recharge.Amount = amount;
            recharge.Member.Balance += amount;

            await _rechargeRepository.UpdateRecharge(recharge);
            var admin = await _accountRepository.FindOne(c => c.Role == Roles.Admin);
            if (admin is null)
            {
                Console.WriteLine("No admin account");
            }
            admin.Balance += recharge.Amount;
            await _accountRepository.UpdateAccount(admin);
            var scheduler = await _schedulerFactory.GetScheduler();
            await scheduler.UnscheduleJob(new TriggerKey($"RechargeExpirationTrigger-{recharge.RechargeId}"));

            return new Result<bool, ErrorCode>(true);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error completing recharge {RechargeId}", rechargeId);
            return new Result<bool, ErrorCode>(ErrorCode.ServerError);
        }
    }

    public async Task<Result<bool, ErrorCode>> FailRecharge(Guid rechargeId)
    {
        try
        {
            var rechargeResult = await GetRechargeById(rechargeId);
            if (!rechargeResult.IsSuccessful)
            {
                return new Result<bool, ErrorCode>(rechargeResult.Error);
            }

            var recharge = rechargeResult.Value;

            if (recharge.Status != RechargeStatus.Pending)
            {
                return new Result<bool, ErrorCode>(ErrorCode.InvalidOperation);
            }

            recharge.Status = RechargeStatus.Failed;

            await _rechargeRepository.UpdateRecharge(recharge);

            var scheduler = await _schedulerFactory.GetScheduler();
            await scheduler.UnscheduleJob(new TriggerKey($"RechargeExpirationTrigger-{recharge.RechargeId}"));

            return new Result<bool, ErrorCode>(true);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error failing recharge {RechargeId}", rechargeId);
            return new Result<bool, ErrorCode>(ErrorCode.ServerError);
        }
    }

    public async Task<Result<RechargePurchaseResponse, ErrorCode>> InitiateRecharge(InitiateRechargeRequest request)
    {
        var recharge = new Recharge
        {
            MemberId = request.MemberId,
            Amount = request.Amount,
            Status = RechargeStatus.Pending,
            CreatedDate = DateTime.UtcNow,
            PaymentMethod = PaymentMethod.Banking
        };

        var rechargeResult = await CreateRecharge(recharge);

        if (!rechargeResult.IsSuccessful)
        {
            return new Result<RechargePurchaseResponse, ErrorCode>(rechargeResult.Error);
        }

        var paymentUrl = _vnPayService.CreatePaymentUrl(
            rechargeResult.Value.RechargeId,
            rechargeResult.Value.Amount,
            $"Recharge account: {rechargeResult.Value.Amount} VND",
            "recharges");

        _logger.LogInformation(
            "Recharge initiated. RechargeId: {RechargeId}, MemberId: {MemberId}, Amount: {Amount} VND",
            rechargeResult.Value.RechargeId, request.MemberId, request.Amount);

        return new Result<RechargePurchaseResponse, ErrorCode>(new RechargePurchaseResponse
        {
            PaymentUrl = paymentUrl,
            RechargeId = rechargeResult.Value.RechargeId
        });
    }

    public async Task<Result<string, ErrorCode>> ProcessPaymentReturn(IQueryCollection requestParams)
    {
        var response = _vnPayService.ProcessPayment(requestParams);
        var redirectUrl = _configuration.GetSection("RedirectUrl").Value + "process-payment";

        if (response.Success)
        {
            try
            {
                var rechargeId = new Guid(response.OrderId);
                var rechargeResult = await GetRechargeById(rechargeId);

                if (!rechargeResult.IsSuccessful)
                {
                    return new Result<string, ErrorCode>($"{redirectUrl}?paymentstatus=error&message={Uri.EscapeDataString(rechargeResult.Error.ToString())}");
                }

                var recharge = rechargeResult.Value;

                if (recharge.Status != RechargeStatus.Pending)
                {
                    _logger.LogWarning("Recharge already processed: {RechargeId}", response.OrderId);
                    return new Result<string, ErrorCode>($"{redirectUrl}?paymentstatus=warning&message={Uri.EscapeDataString("Recharge already processed")}");
                }

                var completeResult = await CompleteRecharge(recharge.RechargeId, recharge.Amount);
                await _transactionService.CreateTransactionFromVnPay(response, TransactionType.AddFund);
                
                if (!completeResult.IsSuccessful)
                {
                    return new Result<string, ErrorCode>($"{redirectUrl}?paymentstatus=error&message={Uri.EscapeDataString("Error completing recharge")}");
                }

                _logger.LogInformation(
                    "Recharge successful. RechargeId: {RechargeId}, Amount: {Amount}", response.OrderId,
                    recharge.Amount);

                return new Result<string, ErrorCode>($"{redirectUrl}?paymentstatus=success&message={Uri.EscapeDataString("Recharge successful")}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing successful payment");
                return new Result<string, ErrorCode>($"{redirectUrl}?paymentstatus=error&message={Uri.EscapeDataString("An error occurred while processing your payment")}");
            }
        }
        else
        {
            try
            {
                var failResult = await FailRecharge(new Guid(response.OrderId));
                if (!failResult.IsSuccessful)
                {
                    _logger.LogError("Failed to mark recharge as failed. RechargeId: {RechargeId}", response.OrderId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking recharge as failed. RechargeId: {RechargeId}", response.OrderId);
            }

            _logger.LogWarning(
                "Payment failed. RechargeId: {RechargeId}, ResponseCode: {VnPayResponseCode}", response.OrderId,
                response.VnPayResponseCode);
            return new Result<string, ErrorCode>($"{redirectUrl}?paymentstatus=error&message={Uri.EscapeDataString("Payment failed")}");
        }
    }
}