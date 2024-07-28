namespace Services.Withdraws;

public interface IWithdrawService
{
    Task<CompleteWithdrawResponse> CompleteWithdrawRequest(Guid withdrawId);
}