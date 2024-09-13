using BusinessObjects.Dtos.Commons;
using BusinessObjects.Dtos.Recharges;
using BusinessObjects.Entities;

namespace Services.Recharges;

public interface IRechargeService
{
    Task<DotNext.Result<Recharge, ErrorCode>> CreateRecharge(Recharge recharge);
    Task<DotNext.Result<Recharge, ErrorCode>> GetRechargeById(Guid rechargeId);
    Task<DotNext.Result<bool, ErrorCode>> CompleteRecharge(Guid rechargeId, decimal amount);

    Task<DotNext.Result<PaginationResponse<RechargeListResponse>, ErrorCode>> GetRecharges(
        GetRechargesRequest request);

    Task<DotNext.Result<bool, ErrorCode>> FailRecharge(Guid rechargeId);
}