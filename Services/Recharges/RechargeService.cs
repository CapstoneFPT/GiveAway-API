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
using Repositories.Recharges;

namespace Services.Recharges;

public class RechargeService : IRechargeService
{
    private readonly IRechargeRepository _rechargeRepository;
    private readonly ILogger<RechargeService> _logger;
    private readonly ISchedulerFactory _schedulerFactory;

    public RechargeService(IRechargeRepository rechargeRepository, ILogger<RechargeService> logger,
        ISchedulerFactory schedulerFactory)

    {
        _rechargeRepository = rechargeRepository;
        _logger = logger;
        _schedulerFactory = schedulerFactory;
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
}