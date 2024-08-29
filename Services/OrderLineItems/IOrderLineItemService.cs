using BusinessObjects.Dtos.Commons;
using BusinessObjects.Dtos.OrderLineItems;
using BusinessObjects.Dtos.Refunds;
using BusinessObjects.Entities;
using DotNext;

namespace Services.OrderLineItems
{
    public interface IOrderLineItemService
    {
        Task<Result<PaginationResponse<OrderLineItemListResponse>, ErrorCode>> GetOrderLineItemsByOrderId(Guid orderId,
            OrderLineItemRequest request);
        Task<Result<OrderLineItemDetailedResponse, ErrorCode>> GetOrderLineItemById(Guid orderId);

        Task<BusinessObjects.Dtos.Commons.Result<RefundResponse>> RequestRefundToShop(CreateRefundRequest refundRequest);

        Task ChangeFashionItemsStatus(List<OrderLineItem> orderDetails, FashionItemStatus fashionItemStatus);
    }
}
