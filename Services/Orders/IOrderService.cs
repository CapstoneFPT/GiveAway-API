using BusinessObjects.Dtos.Commons;
using BusinessObjects.Dtos.FashionItems;
using BusinessObjects.Dtos.Orders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessObjects.Dtos.Auctions;

namespace Services.Orders
{
    public interface IOrderService
    {
        Task<Result<PaginationResponse<OrderResponse>>> GetOrdersByAccountId(Guid accountId, OrderRequest request);
        Task<Result<OrderResponse>> CreateOrder(Guid accountId,List<Guid?> listItemId, CreateOrderRequest order);
        Task<Result<string>> CancelOrder(Guid orderId);
        Task<Result<PaginationResponse<OrderResponse>>> GetOrdersByShopId(Guid shopId, OrderRequest orderRequest);
        Task<Result<string>> ConfirmOrderDeliveried(Guid orderId);
        Task<Result<OrderResponse>> CreateOrderFromBid(CreateOrderFromBidRequest orderRequest);
    }
}
