using BusinessObjects.Dtos.Commons;
using BusinessObjects.Dtos.Recharges;
using BusinessObjects.Entities;
using DotNext;
using Microsoft.AspNetCore.Http;
using Services.VnPayService;
using Services.Transactions;

namespace Services.Recharges;

public interface IRechargeService
{
    Task<DotNext.Result<Recharge, ErrorCode>> CreateRecharge(Recharge recharge);
    Task<DotNext.Result<Recharge, ErrorCode>> GetRechargeById(Guid rechargeId);
    Task<DotNext.Result<bool, ErrorCode>> CompleteRecharge(Guid rechargeId, decimal amount);

    Task<DotNext.Result<PaginationResponse<RechargeListResponse>, ErrorCode>> GetRecharges(
        GetRechargesRequest request);

    Task<DotNext.Result<bool, ErrorCode>> FailRecharge(Guid rechargeId);

    Task<Result<RechargePurchaseResponse, ErrorCode>> InitiateRecharge(InitiateRechargeRequest request);
    Task<Result<string, ErrorCode>> ProcessPaymentReturn(IQueryCollection requestParams);
}