namespace Repositories.Revenues;

public interface IRevenueRepository 
{
    Task<decimal> GetConsignorPayouts(Guid? shopId, DateTime startDate, DateTime endDate);
    Task<decimal> GetTotalRevenue(Guid? shopId, DateTime startDate, DateTime endDate);
    Task<decimal> GetTotalPayout(Guid? shopId, DateTime startDate, DateTime endDate);
}