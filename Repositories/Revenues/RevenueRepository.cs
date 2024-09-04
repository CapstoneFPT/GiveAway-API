using BusinessObjects.Dtos.Commons;
using BusinessObjects.Entities;
using Dao;
using Microsoft.EntityFrameworkCore;

namespace Repositories.Revenues;

public class RevenueRepository : IRevenueRepository
{
   
    private const decimal ConsignPayoutPercentage = 0.9m;

  
  

    public async Task<decimal> GetTotalRevenue(Guid? shopId, DateTime startDate, DateTime endDate)
    {
        var orderRevenue = await GetOrderRevenue(shopId, startDate, endDate);
        return orderRevenue;
    }

    private async Task<decimal> GetOrderRevenue(Guid? shopId, DateTime startDate, DateTime endDate)
    {
        var query = GenericDao<OrderLineItem>.Instance.GetQueryable()
            .Where(od => od.Order.CreatedDate >= startDate &&
                         od.Order.CreatedDate <= endDate &&
                         od.Order.Status == OrderStatus.Completed);

        if (shopId.HasValue)
        {
            query = query.Where(od => od.IndividualFashionItem.MasterItem.ShopId == shopId.Value);
        }

        return await query.SumAsync(od => od.UnitPrice);
    }


    public async Task<decimal?> GetConsignSaleRevenue(Guid? shopId, DateTime startDate, DateTime endDate)
    {
        var query = GenericDao<ConsignSaleLineItem>.Instance
                .GetQueryable()
                .Where(detail =>
                    detail.ConsignSale.CreatedDate >= startDate &&
                    detail.ConsignSale.CreatedDate <= endDate &&
                    detail.ConsignSale.Status == ConsignSaleStatus.Completed)
            ;

        if (shopId.HasValue)
        {
            query = query.Where(detail => detail.ConsignSale.ShopId == shopId.Value);
        }

        var result = await query
            .SumAsync(detail => detail.ConfirmedPrice);
        return result;
    }

    public Task<decimal> GetTotalPayout(Guid? shopId, DateTime startDate, DateTime endDate)
    {
        var query = GenericDao<OrderLineItem>.Instance.GetQueryable()
            .Where(od => od.Order.CreatedDate >= startDate &&
                         od.Order.CreatedDate <= endDate &&
                         od.Order.Status == OrderStatus.Completed &&
                         od.IndividualFashionItem.Type == FashionItemType.ConsignedForSale);

        if (shopId.HasValue)
        {
            // query = query.Where(od => od.IndividualFashionItem.ShopId == shopId.Value);
        }

        return query.SumAsync(od => od.UnitPrice * ConsignPayoutPercentage);
    }

    public async Task<decimal> GetConsignorPayouts(Guid? shopId, DateTime startDate, DateTime endDate)
    {
        var query = GenericDao<ConsignSale>.Instance.GetQueryable()
                .Where(consignSale => consignSale.ShopId == shopId &&
                                      consignSale.CreatedDate >= startDate &&
                                      consignSale.CreatedDate <= endDate &&
                                      consignSale.Status == ConsignSaleStatus.Completed)
            ;

        if (shopId.HasValue)
        {
            query = query.Where(consignSale => consignSale.ShopId == shopId.Value);
        }

        var result = await query
            .SumAsync(consignSale => consignSale.ConsignorReceivedAmount);

        return result;
    }
}