using BusinessObjects.Dtos.Commons;
using BusinessObjects.Dtos.FashionItems;
using BusinessObjects.Dtos.Orders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessObjects.Dtos.AuctionDeposits;
using BusinessObjects.Dtos.Auctions;
using BusinessObjects.Dtos.OrderLineItems;
using BusinessObjects.Entities;
using DotNext;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Services.GiaoHangNhanh;

namespace Services.Orders
{
    public interface IOrderService
    {
        Task<Result<PaginationResponse<OrderListResponse>, ErrorCode>> GetOrdersByAccountId(Guid accountId,
            OrderRequest request);

        Task<Result<ExcelResponse, ErrorCode>> ExportOrdersToExcel(ExportOrdersToExcelRequest request);
        Task<Result<InvoiceResponse, ErrorCode>> GenerateInvoice(Guid orderId, Guid shopId);
        Task<BusinessObjects.Dtos.Commons.Result<OrderResponse>> CreateOrder(Guid accountId, CartRequest cart);
        Task<BusinessObjects.Dtos.Commons.Result<string>> CancelOrder(Guid orderId);
        Task<BusinessObjects.Dtos.Commons.Result<string>> CancelOrderByAdmin(Guid orderId);
        Task<Result<PaginationResponse<OrderListResponse>, ErrorCode>> GetOrders(OrderRequest orderRequest);
        Task<BusinessObjects.Dtos.Commons.Result<OrderResponse>> ConfirmOrderDeliveried(Guid shopId, Guid orderId);

        Task<BusinessObjects.Dtos.Commons.Result<OrderResponse>> CreateOrderFromBid(
            CreateOrderFromBidRequest orderRequest);

        Task<Order?> GetOrderById(Guid orderId);
        Task UpdateOrder(Order? order);
        Task<List<Order?>> GetOrdersToCancel();
        Task CancelOrders(List<Order?> ordersToCancel);
        Task UpdateShopBalance(Order order);
        Task UpdateFashionItemStatus(Guid orderOrderId);
        Task PayWithPoints(Guid orderId, Guid requestMemberId);

        Task<BusinessObjects.Dtos.Commons.Result<OrderResponse>> CreateOrderByShop(Guid shopId,
            CreateOrderRequest request);

        Task<DotNext.Result<PayOrderOfflineResponse, ErrorCode>> OfflinePay(Guid shopId, Guid orderId);

        Task UpdateAdminBalance(Order order);

        Task<BusinessObjects.Dtos.Commons.Result<OrderResponse>> ConfirmPendingOrder(Guid orderdetailId,
            ConfirmPendingOrderRequest itemStatus);

        Task<DotNext.Result<ShippingFeeResult, ErrorCode>> CalculateShippingFee(List<Guid> itemIds,
            int destinationDistrictId);

        Task<Result<OrderDetailedResponse, ErrorCode>> GetDetailedOrder(Guid orderId);

        Task<DotNext.Result<PaginationResponse<OrderLineItemListResponse>, ErrorCode>> GetOrderLineItemByOrderId(
            Guid orderId, OrderLineItemRequest request);


        Task<DotNext.Result<VnPayPurchaseResponse,ErrorCode>> PurchaseOrder(Guid orderId, PurchaseOrderRequest request);
        Task<DotNext.Result<string,ErrorCode>> PaymentReturn(IQueryCollection requestParams);
        Task<DotNext.Result<PayWithPointsResponse,ErrorCode>> PurchaseOrderWithPoints(Guid orderId, PurchaseOrderRequest request);

        Task<DotNext.Result<PayWithPointsResponse, ErrorCode>> CheckoutAuction(Guid orderId,
            CheckoutAuctionRequest request);
    }
}