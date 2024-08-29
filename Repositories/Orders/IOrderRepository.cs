using BusinessObjects.Dtos.Commons;
using BusinessObjects.Dtos.Orders;
using BusinessObjects.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.Orders
{
    public interface IOrderRepository
    {
        Task<PaginationResponse<OrderResponse>> GetOrdersByAccountId(Guid accountId, OrderRequest request);
        Task<Order?> CreateOrder(Order? order);
        Task<Order?> UpdateOrder(Order? order);
        Task<Order?> GetOrderById(Guid id);
        Task<OrderResponse> CreateOrderHierarchy(Guid accountId ,CartRequest cart);
        Task<List<OrderLineItem>> IsOrderExisted(List<Guid> listItemId, Guid memberid);
        Task<List<Guid>> IsOrderAvailable(List<Guid> listItemId);
        Task<PaginationResponse<OrderResponse>> GetOrders(OrderRequest orderRequest);
        Task<Order?> GetSingleOrder(Expression<Func<Order,bool>> predicate);
        /*Task<Order> ConfirmOrderDelivered(Guid shopId,Guid orderId);*/
        Task<List<Order?>> GetOrders(Expression<Func<Order?, bool>> predicate);
        Task BulkUpdate(List<Order?> ordersToUpdate);
        Task<OrderResponse> CreateOrderByShop(Guid shopId, CreateOrderRequest orderRequest);
        string GenerateUniqueString();
        Task<(List<T> Items, int Page, int PageSize, int TotalCount)> GetOrdersProjection<T>(int? orderRequestPageNumber, int? orderRequestPageSize, Expression<Func<Order,bool>> predicate, Expression<Func<Order,T>> selector);
        IQueryable<Order> GetQueryable();
    }
}
