using BusinessObjects.Dtos.Commons;
using BusinessObjects.Dtos.Withdraws;
using BusinessObjects.Entities;
using DotNext;

namespace Services.Withdraws;

public interface IWithdrawService
{
    Task<CompleteWithdrawResponse> CompleteWithdrawRequest(Guid withdrawId);
    Task ScheduleWithdrawExpiration(Withdraw withdraw);
    Task<Result<PaginationResponse<GetWithdrawsResponse>, ErrorCode>> GetAllPaginationWithdraws(
        GetWithdrawByAdminRequest request);
}