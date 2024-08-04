using BusinessObjects.Dtos.Commons;
using BusinessObjects.Dtos.FashionItems;
using BusinessObjects.Dtos.Orders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessObjects.Dtos.Auctions;
using BusinessObjects.Entities;

namespace Services.Orders
{
    public interface IOrderService
    {
        Task<Result<PaginationResponse<OrderListResponse>>> GetOrdersByAccountId(Guid accountId, OrderRequest request);
        Task<Result<OrderResponse>> CreateOrder(Guid accountId, CartRequest cart);
        Task<Result<string>> CancelOrder(Guid orderId);
        Task<Result<string>> CancelOrderByShop(Guid shopId,Guid orderId);
        Task<Result<PaginationResponse<OrderListResponse>>> GetOrders(OrderRequest orderRequest);
        Task<Result<OrderResponse>> ConfirmOrderDeliveried(Guid orderId);
        Task<Result<OrderResponse>> CreateOrderFromBid(CreateOrderFromBidRequest orderRequest);
        Task<Result<OrderResponse>> CreatePointPackageOrder(PointPackageOrder order);
        Task<Order?> GetOrderById(Guid orderId);
        Task UpdateOrder(Order? order);
        Task<List<OrderDetail>> GetOrderDetailByOrderId(Guid orderId);
        Task<List<Order?>> GetOrdersToCancel();
        Task CancelOrders(List<Order?> ordersToCancel);
        Task UpdateShopBalance(Order order);
        Task UpdateFashionItemStatus(Guid orderOrderId);
        Task PayWithPoints(Guid orderId, Guid requestMemberId);
        Task<Result<OrderResponse>> CreateOrderByShop(Guid shopId, CreateOrderRequest request);
        Task<PayOrderWithCashResponse> PayWithCash(Guid shopId, Guid orderId, PayOrderWithCashRequest request);
        
        Task UpdateAdminBalance(Order order);
        Task<Result<OrderResponse>> ConfirmPendingOrder(Guid orderId ,Guid orderdetailId);
    }
}
