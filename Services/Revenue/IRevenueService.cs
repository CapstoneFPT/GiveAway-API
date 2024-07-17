using BusinessObjects.Dtos.ConsignSales;

namespace Services.Revenue;

public interface IRevenueService
{
    Task<ShopRevenueDto> GetShopRevenue(Guid shopId,DateTime startDate, DateTime endDate);
    Task<SystemRevenueDto> GetSystemRevenue(DateTime startDate, DateTime endDate);
    Task<MonthlyRevenueDto> GetSystemMonthlyRevenue(int year);
    Task<MonthlyPayoutsResponse> GetMonthlyPayouts(int year, Guid? shopId);
    Task<MonthlyRevenueDto> GetShopMonthlyOfflineRevenue(Guid shopId, int year);
}