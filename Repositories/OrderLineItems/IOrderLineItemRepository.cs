using System.Linq.Expressions;
using BusinessObjects.Dtos.Commons;
using BusinessObjects.Dtos.OrderLineItems;
using BusinessObjects.Dtos.Refunds;
using BusinessObjects.Entities;
using DotNext;

namespace Repositories.OrderLineItems
{
    public interface IOrderLineItemRepository
    {
        Task<Result<PaginationResponse<OrderLineItemListResponse>, ErrorCode>> GetAllOrderLineItemsByOrderId(Guid id,
            OrderLineItemRequest request);
        Task<List<OrderLineItem>> GetOrderLineItems(Expression<Func<OrderLineItem, bool>> predicate);
        Task<OrderLineItem> GetOrderLineItemById(Guid id);
        Task<OrderLineItem> CreateOrderLineItem(OrderLineItem orderLineItem);
        Task<RefundResponse> CreateRefundToShop(CreateRefundRequest refundRequest);
      

        Task<(List<T> Items, int Page, int PageSize, int TotalCount)>
            GetOrderLineItemsPaginate<T>(Expression<Func<OrderLineItem, bool>>? predicate,
                Expression<Func<OrderLineItem, T>>? selector, bool isTracking, int page = -1, int pageSize = -1);
        Task UpdateRange(List<OrderLineItem> orderLineItems);
        Task UpdateOrderLine(OrderLineItem orderLineItem);
         IQueryable<OrderLineItem> GetQueryable();
    }
}
