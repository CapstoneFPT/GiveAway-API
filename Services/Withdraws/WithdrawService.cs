using System.Linq.Expressions;
using BusinessObjects.Dtos.Commons;
using BusinessObjects.Dtos.Withdraws;
using BusinessObjects.Entities;
using BusinessObjects.Utils;
using DotNext;
using LinqKit;
using Repositories.Accounts;
using Repositories.Transactions;
using Repositories.Withdraws;

namespace Services.Withdraws;

public class WithdrawService : IWithdrawService
{
    private readonly IWithdrawRepository _withdrawRepository;
    private readonly ITransactionRepository _transactionRepository;
    private readonly IAccountRepository _accountRepository;

    public WithdrawService(IWithdrawRepository withdrawRepository, ITransactionRepository transactionRepository,
        IAccountRepository accountRepository)
    {
        _withdrawRepository = withdrawRepository;
        _transactionRepository = transactionRepository;
        _accountRepository = accountRepository;
    }

    public async Task<CompleteWithdrawResponse> CompleteWithdrawRequest(Guid withdrawId)
    {
        Expression<Func<Withdraw, bool>> predicate = x => x.WithdrawId == withdrawId;
        var admin = await _accountRepository.FindOne(x => x.Role == Roles.Admin);
        var withdraw = await _withdrawRepository.GetSingleWithdraw(predicate);

        if (withdraw == null)
        {
            throw new WithdrawNotFoundException();
        }

        if (withdraw.Status != WithdrawStatus.Processing)
        {
            throw new AnomalousWithdrawStatusException("Withdraw status is not Pending");
        }

        withdraw.Status = WithdrawStatus.Completed;
        admin.Balance -= withdraw.Amount;

        await _withdrawRepository.UpdateWithdraw(withdraw);
        await _accountRepository.UpdateAccount(admin);
        
        return new CompleteWithdrawResponse()
        {
            WithdrawId = withdrawId,
            Status = withdraw.Status,
            Amount = withdraw.Amount,
            CreatedDate = withdraw.CreatedDate,
            MemberId = withdraw.MemberId
        };
    }

    public async Task<Result<PaginationResponse<GetWithdrawsResponse>, ErrorCode>> GetAllPaginationWithdraws(GetWithdrawByAdminRequest request)
    {
        Expression<Func<Withdraw, GetWithdrawsResponse>> selector = withdraw => new GetWithdrawsResponse()
        {
            WithdrawId = withdraw.WithdrawId,
            MemberId = withdraw.MemberId,
            CreatedDate = withdraw.CreatedDate,
            Amount = withdraw.Amount,
            Status = withdraw.Status,
            Bank = withdraw.Bank,
            BankAccountName = withdraw.BankAccountName,
            BankAccountNumber = withdraw.BankAccountNumber,
            WithdrawCode = withdraw.WithdrawCode
        };
        Expression<Func<Withdraw, bool>> predicate = withdraw => true;
        if (request.WithdrawCode != null)
        {
            predicate = predicate.And(c => c.WithdrawCode == request.WithdrawCode);
        }

        if (request.MemberId != null)
        {
            predicate = predicate.And(c => c.MemberId == request.MemberId);
        }

        if (request.Status != null)
        {
            predicate = predicate.And(c => c.Status.Equals(request.Status));
        }
        Expression<Func<Withdraw, DateTime>> orderBy = withdraw => withdraw.CreatedDate;
        (List<GetWithdrawsResponse> Items, int Page, int PageSize, int TotalCount) data =
            await _withdrawRepository.GetWithdraws(request.Page, request.PageSize, predicate, selector, false, orderBy);
        return new PaginationResponse<GetWithdrawsResponse>()
        {
            Items = data.Items,
            PageSize = data.PageSize,
            PageNumber = data.Page,
            TotalCount = data.TotalCount
        };
    }
}

public class CompleteWithdrawResponse
{
    public Guid WithdrawId { get; set; }
    public WithdrawStatus Status { get; set; }
    public int Amount { get; set; }
    public DateTime CreatedDate { get; set; }
    public Guid MemberId { get; set; }
}