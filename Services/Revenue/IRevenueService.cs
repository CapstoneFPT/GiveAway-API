using BusinessObjects.Dtos.ConsignSales;

namespace Services.Revenue;

public interface IRevenueService
{
    Task<ShopRevenueDto> GetShopRevenue(Guid shopId,DateTime startDate, DateTime endDate);
    Task<SystemRevenueDto> GetSystemRevenue(DateTime startDate, DateTime endDate);
    Task<MonthlyRevenueDto> GetMonthlyRevenue(int year, Guid? shopId);
    Task<MonthlyPayoutsResponse> GetMonthlyPayouts(int year, Guid? shopId);
}