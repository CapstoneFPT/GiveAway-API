using BusinessObjects.Entities;

namespace Repositories.Recharges;

public interface IRechargeRepository
{
    Task<Recharge?> CreateRecharge(Recharge recharge);
    Task<Recharge?> GetRechargeById(Guid rechargeId);
    Task UpdateRecharge(Recharge recharge);
     IQueryable<Recharge> GetQueryable();
}