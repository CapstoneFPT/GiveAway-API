using System.Linq.Expressions;
using BusinessObjects.Dtos.ConsignSaleLineItems;
using BusinessObjects.Entities;

namespace Repositories.ConsignSaleLineItems;

public interface IConsignSaleLineItemRepository
{
    Task<List<ConsignSaleLineItemsListResponse>> GetConsignSaleLineItemsByConsignSaleId(Guid consignSaleId);
    Task<ConsignSaleLineItem?> GetSingleConsignSaleLineItem(Expression<Func<ConsignSaleLineItem, bool>> predicate);
    Task UpdateConsignLineItem(ConsignSaleLineItem consignSaleLineItem);
    IQueryable<ConsignSaleLineItem> GetQueryable();
    Task AddConsignSaleLineItem(ConsignSaleLineItem consignSaleLineItem);
}