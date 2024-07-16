using BusinessObjects.Dtos.Commons;
using BusinessObjects.Entities;
using Dao;
using Microsoft.EntityFrameworkCore;

namespace Repositories.Revenues;

public class RevenueRepository : IRevenueRepository
{
    private readonly GenericDao<OrderDetail> _orderDetailDao;
    private readonly GenericDao<ConsignSaleDetail> _consignSaleDetailDao;
    private readonly GenericDao<ConsignSale> _consignSaleDao;
    private const decimal ConsignPayoutPercentage = 0.9m;

    public RevenueRepository(GenericDao<OrderDetail> orderDetailDao, GenericDao<ConsignSaleDetail> consignSaleDetailDao,
        GenericDao<ConsignSale> consignSaleDao)
    {
        _orderDetailDao = orderDetailDao;
        _consignSaleDetailDao = consignSaleDetailDao;
        _consignSaleDao = consignSaleDao;
    }

    public async Task<decimal> GetDirectSaleRevenue(Guid? shopId, DateTime startDate, DateTime endDate)
    {
        var query = _orderDetailDao
                .GetQueryable()
                .Include(x=>x.Order)
                .Where(detail =>
                    detail.Order.CreatedDate >= startDate &&
                    detail.Order.CreatedDate <= endDate &&
                    detail.Order.Status == OrderStatus.Completed)
            ;
        
        

        if (shopId.HasValue)
        {
            query = query.Where(detail => detail.FashionItem.ShopId == shopId.Value && detail.Order.PurchaseType == PurchaseType.Offline);
        }

        var list = await query.ToListAsync();

        var result = await query
            .SumAsync(detail => detail.UnitPrice);
        return result;
    }

    public async Task<decimal> GetTotalRevenue(Guid? shopId, DateTime startDate, DateTime endDate)
    {
        var orderRevenue = await GetOrderRevenue(shopId, startDate, endDate);
        return orderRevenue;
    }

    private async Task<decimal> GetOrderRevenue(Guid? shopId, DateTime startDate, DateTime endDate)
    {
        var query = _orderDetailDao.GetQueryable()
            .Where(od => od.Order.CreatedDate >= startDate &&
                         od.Order.CreatedDate <= endDate &&
                         od.Order.Status == OrderStatus.Completed);

        if (shopId.HasValue)
        {
            query = query.Where(od => od.FashionItem.ShopId == shopId.Value);
        }

        return await query.SumAsync(od => od.UnitPrice);
    }


    public async Task<decimal> GetConsignSaleRevenue(Guid? shopId, DateTime startDate, DateTime endDate)
    {
        var query = _consignSaleDetailDao
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
        var query = _orderDetailDao.GetQueryable()
            .Where(od => od.Order.CreatedDate >= startDate &&
                         od.Order.CreatedDate <= endDate &&
                         od.Order.Status == OrderStatus.Completed &&
                         od.FashionItem.Type == FashionItemType.ConsignedForSale);

        if (shopId.HasValue)
        {
            query = query.Where(od => od.FashionItem.ShopId == shopId.Value);
        }

        return query.SumAsync(od => od.UnitPrice * ConsignPayoutPercentage);
    }

    public async Task<decimal> GetConsignorPayouts(Guid? shopId, DateTime startDate, DateTime endDate)
    {
        var query = _consignSaleDao.GetQueryable()
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
            .SumAsync(consignSale => consignSale.MemberReceivedAmount);

        return result;
    }
}