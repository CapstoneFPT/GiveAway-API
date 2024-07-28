using System.Linq.Expressions;
using BusinessObjects.Dtos.Commons;
using BusinessObjects.Entities;
using BusinessObjects.Utils;
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

        await _withdrawRepository.UpdateWithdraw(withdraw);
        
        return new CompleteWithdrawResponse()
        {
            WithdrawId = withdrawId,
            Status = withdraw.Status,
            Amount = withdraw.Amount,
            CreatedDate = withdraw.CreatedDate,
            MemberId = withdraw.MemberId
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