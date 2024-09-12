using BusinessObjects.Dtos.Commons;
using BusinessObjects.Dtos.Withdraws;
using DotNext;

namespace Services.Withdraws;

public interface IWithdrawService
{
    Task<CompleteWithdrawResponse> CompleteWithdrawRequest(Guid withdrawId);

    Task<Result<PaginationResponse<GetWithdrawsResponse>, ErrorCode>> GetAllPaginationWithdraws(
        GetWithdrawByAdminRequest request);
}