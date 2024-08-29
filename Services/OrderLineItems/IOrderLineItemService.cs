using BusinessObjects.Dtos.Commons;
using BusinessObjects.Dtos.OrderLineItems;
using BusinessObjects.Dtos.Refunds;
using BusinessObjects.Entities;

namespace Services.OrderLineItems
{
    public interface IOrderLineItemService
    {
        Task<Result<PaginationResponse<OrderLineItemDetailedResponse>>> GetOrderLineItemsByOrderId(Guid orderId, OrderLineItemRequest request);
        Task<Result<OrderLineItemResponse<IndividualFashionItem>>> GetOrderLineItemById(Guid orderId);

        Task<Result<RefundResponse>> RequestRefundToShop(CreateRefundRequest refundRequest);

        Task ChangeFashionItemsStatus(List<OrderLineItem> orderDetails, FashionItemStatus fashionItemStatus);
    }
}
