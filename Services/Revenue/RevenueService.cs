using BusinessObjects.Dtos.ConsignSales;
using Repositories.Revenues;

namespace Services.Revenue;

public class RevenueService : IRevenueService
{
    private readonly IRevenueRepository _revenueRepository;

    public RevenueService(IRevenueRepository revenueRepository)
    {
        _revenueRepository = revenueRepository;
    }

    public async Task<ShopRevenueDto> GetShopRevenue(Guid shopId,DateTime startDate, DateTime endDate)
    {
       var directSalesRevenue = await _revenueRepository.GetTotalRevenue(shopId, startDate, endDate);
       var consignorPayouts = await _revenueRepository.GetConsignorPayouts(shopId, startDate, endDate);
       
       return new ShopRevenueDto
       {
           ShopId = shopId,
           TotalRevenue = directSalesRevenue,
           DirectSalesRevenue = directSalesRevenue,
           ConsignorPayouts = consignorPayouts,
           StartDate = startDate,
           EndDate = endDate
       };
    } 
    
    public async Task<SystemRevenueDto> GetSystemRevenue(DateTime startDate, DateTime endDate)
    {
        var revenue = await _revenueRepository.GetTotalRevenue(null, startDate, endDate);

        return new SystemRevenueDto
        {
            TotalRevenue = revenue,
            StartDate = startDate,
            EndDate = endDate
        };
    }

    public async Task<MonthlyPayoutsResponse> GetMonthlyPayouts(int year, Guid? shopId)
    {
        var monthlyPayouts = new List<MonthPayout>();
        for (int month = 1; month <= 12; month++)
        {
            var startDate = new DateTime(year, month, 1);
            var endDate = startDate.AddMonths(1).AddDays(-1);

            var payout = await _revenueRepository.GetTotalPayout(shopId, startDate, endDate);
            monthlyPayouts.Add(new MonthPayout { Month = month, ConsignPayout = payout });
        }
        
        return new MonthlyPayoutsResponse
        {
            Year = year,
            MonthlyPayouts = monthlyPayouts
        };
    }

    public async Task<MonthlyRevenueDto> GetSystemMonthlyRevenue(int year)
    {
        var monthlyRevenue = new List<MonthRevenue>();
        for (int month = 1; month <= 12; month++)
        {
            var startDate = new DateTime(year, month, 1);
            var endDate = startDate.AddMonths(1).AddDays(-1);
                
            var revenue = await _revenueRepository.GetTotalRevenue(null, startDate, endDate);
            monthlyRevenue.Add(new MonthRevenue { Month = month, Revenue = revenue });
        }

        return new MonthlyRevenueDto
        {
            Year = year,
            MonthlyRevenue = monthlyRevenue
        };
    }

    public async Task<MonthlyRevenueDto> GetShopMonthlyOfflineRevenue(Guid shopId, int year)
    {
        var monthlyRevenue = new List<MonthRevenue>();
        for (int month = 1; month <= 12; month++)
        {
            var startDate = new DateTime(year, month, 1);
            var endDate = startDate.AddMonths(1).AddDays(-1);
                
            var revenue = await _revenueRepository.GetTotalRevenue(shopId, startDate, endDate);
            monthlyRevenue.Add(new MonthRevenue { Month = month, Revenue = revenue });
        }
        
        return new MonthlyRevenueDto
        {
            Year = year,
            MonthlyRevenue = monthlyRevenue
        };
    }
}

public class MonthRevenue
{
    public int Month { get; set; }
    public decimal Revenue { get; set; }
}

public class MonthlyRevenueDto
{
    public int Year { get; set; }
    public List<MonthRevenue> MonthlyRevenue { get; set; }
}

public class SystemRevenueDto
{
    public decimal TotalRevenue { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}

public class ShopRevenueDto
{
    public Guid ShopId { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal DirectSalesRevenue { get; set; }
    public decimal ConsignorPayouts { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}