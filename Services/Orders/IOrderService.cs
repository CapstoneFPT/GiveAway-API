﻿using BusinessObjects.Dtos.Commons;
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
        Task<Result<PaginationResponse<OrderResponse>>> GetOrdersByAccountId(Guid accountId, OrderRequest request);
        Task<Result<OrderResponse>> CreateOrder(Guid accountId, CreateOrderRequest order);
        Task<Result<string>> CancelOrder(Guid orderId);
        Task<Result<PaginationResponse<OrderResponse>>> GetOrdersByShopId(Guid shopId, OrderRequest orderRequest);
        Task<Result<OrderResponse>> ConfirmOrderDeliveried(Guid shopId ,Guid orderId);
        Task<Result<OrderResponse>> CreateOrderFromBid(CreateOrderFromBidRequest orderRequest);
        Task<Result<OrderResponse>> CreatePointPackageOrder(PointPackageOrder order);
        Task<Order?> GetOrderById(Guid orderId);
        Task UpdateOrder(Order order);
        Task<List<OrderDetail>> GetOrderDetailByOrderId(Guid orderId);
        Task<List<Order>> GetOrdersToCancel();
        void CancelOrders(List<Order> ordersToCancel);
        Task UpdateShopBalance(Order order);
        Task UpdateFashionItemStatus(Guid orderOrderId);
        Task PayWithPoints(Guid orderId, Guid requestMemberId);
    }
}
