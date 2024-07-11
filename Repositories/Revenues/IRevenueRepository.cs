namespace Repositories.Revenues;

public interface IRevenueRepository 
{
    Task<decimal> GetConsignorPayouts(Guid? shopId, DateTime startDate, DateTime endDate);
    Task<decimal> GetConsignSaleRevenue(Guid? shopId, DateTime startDate, DateTime endDate);
    Task<decimal> GetDirectSaleRevenue(Guid? shopId, DateTime startDate, DateTime endDate);
    Task<decimal> GetTotalRevenue(Guid? shopId, DateTime startDate, DateTime endDate);
    Task<decimal> GetTotalPayout(Guid? shopId, DateTime startDate, DateTime endDate);
}