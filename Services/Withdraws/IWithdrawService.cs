namespace Services.Withdraws;

public interface IWithdrawService
{
    Task<ApproveWithdrawResponse> ApproveWithdraw(Guid withdrawId);
}