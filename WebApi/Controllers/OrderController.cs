using System.Net;
using BusinessObjects.Dtos.AuctionDeposits;
using BusinessObjects.Dtos.Commons;
using BusinessObjects.Dtos.OrderLineItems;
using BusinessObjects.Dtos.Orders;
using BusinessObjects.Entities;
using BusinessObjects.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Services.Accounts;
using Services.ConsignSales;
using Services.Emails;
using Services.GiaoHangNhanh;
using Services.OrderLineItems;
using Services.Orders;
using Services.Transactions;
using Services.VnPayService;

namespace WebApi.Controllers
{
    [Authorize]
    [Route("api/orders")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _orderService;
        

        public OrderController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        [HttpGet]
        [ProducesResponseType<PaginationResponse<OrderListResponse>>((int)HttpStatusCode.OK)]
        [ProducesResponseType<ErrorResponse>((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetOrders(
            [FromQuery] OrderRequest orderRequest)
        {
            var result = await _orderService.GetOrders(orderRequest);

            if (!result.IsSuccessful)
            {
                return result.Error switch
                {
                    _ => StatusCode(500,
                        new ErrorResponse("Error fetching orders", ErrorType.ApiError,
                            HttpStatusCode.InternalServerError, result.Error))
                };
            }

            return Ok(result.Value);
        }

        [HttpGet("{orderId}/orderlineitems")]
        [ProducesResponseType<PaginationResponse<OrderLineItemListResponse>>((int)HttpStatusCode.OK)]
        [ProducesResponseType<ErrorResponse>((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult>
            GetOrderLineItemByOrderId([FromRoute] Guid orderId, [FromQuery] OrderLineItemRequest request)
        {
            var result = await _orderService.GetOrderLineItemByOrderId(orderId, request);

            if (!result.IsSuccessful)
            {
                return result.Error switch
                {
                    _ => StatusCode(500, new ErrorResponse("Error fetching order line items", ErrorType.ApiError,
                        HttpStatusCode.InternalServerError, result.Error))
                };
            }

            return Ok(result.Value);
        }

        [HttpGet("{orderId}")]
        [ProducesResponseType<OrderDetailedResponse>((int)HttpStatusCode.OK)]
        [ProducesResponseType<ErrorResponse>((int)HttpStatusCode.InternalServerError)]
        [ProducesResponseType<ErrorResponse>((int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> GetOrderById([FromRoute] Guid orderId)
        {
            var result = await _orderService.GetDetailedOrder(orderId);

            if (!result.IsSuccessful)
            {
                return result.Error switch
                {
                    ErrorCode.NotFound => NotFound(new ErrorResponse("Order not found", ErrorType.ApiError,
                        HttpStatusCode.NotFound, result.Error)),
                    _ => StatusCode(500,
                        new ErrorResponse("Error fetching order", ErrorType.ApiError,
                            HttpStatusCode.InternalServerError, result.Error))
                };
            }

            return Ok(result.Value);
        }
        [AllowAnonymous]
        [HttpPost("{orderId}/pay/vnpay")]
        [ProducesResponseType<VnPayPurchaseResponse>((int)HttpStatusCode.OK)]
        public async Task<IActionResult> PurchaseOrder([FromRoute] Guid orderId,
            [FromBody] PurchaseOrderRequest request)
        {
            var result = await _orderService.PurchaseOrder(orderId, request);
            return Ok(result.Value);
        }
        [AllowAnonymous]
        [HttpGet("payment-return")]
        public async Task<IActionResult> PaymentReturn()
        {
            var requestParams = Request.Query;
            var result = await _orderService.PaymentReturn(requestParams);
            return Redirect(result.Value);
        }

        [HttpPost("{orderId}/pay/points")]
        [ProducesResponseType<PayWithPointsResponse>((int)HttpStatusCode.Created)]
        public async Task<IActionResult> PurchaseOrderWithPoints([FromRoute] Guid orderId,
            [FromBody] PurchaseOrderRequest request)
        {
            var result = await _orderService.PurchaseOrderWithPoints(orderId, request);

            if (!result.IsSuccessful)
            {
                return result.Error switch
                {
                    _ => StatusCode(500,
                        new ErrorResponse("Error purchasing order", ErrorType.ApiError,
                            HttpStatusCode.InternalServerError, result.Error))
                };
            }
            
            return Ok(result.Value);
        }

        [HttpPut("{OrderId}/cancel")]
        public async Task<ActionResult<Result<string>>> CancelOrder([FromRoute] Guid OrderId)
        {
            return await _orderService.CancelOrder(OrderId);
        }
        [Authorize(Roles = "Admin")]
        [HttpPut("{orderId}/cancelbyadmin")]
        public async Task<ActionResult<Result<string>>> CancelOrderByAdmin([FromRoute] Guid orderId)
        {
            var result = await _orderService.CancelOrderByAdmin(orderId);

            if (result.ResultStatus != ResultStatus.Success)
                return StatusCode((int)HttpStatusCode.InternalServerError, result);

            return Ok(result);
        }
        [Authorize(Roles = "Staff,Admin")]
        [HttpGet("{orderId}/invoice")]
        [ProducesResponseType(typeof(FileContentResult), (int)HttpStatusCode.OK)]
        [ProducesResponseType<ErrorResponse>((int)HttpStatusCode.NotFound)]
        [ProducesResponseType<ErrorResponse>((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GenerateOrderInvoice([FromRoute] Guid orderId, [FromQuery] Guid shopId)
        {
            var result = await _orderService.GenerateInvoice(orderId, shopId);

            if (!result.IsSuccessful)
            {
                return result.Error switch
                {
                    ErrorCode.NotFound => NotFound(new ErrorResponse("Order not found", ErrorType.ApiError,
                        HttpStatusCode.NotFound, result.Error)),
                    _ => StatusCode(500,
                        new ErrorResponse("Error generating invoice", ErrorType.ApiError,
                            HttpStatusCode.InternalServerError, result.Error))
                };
            }

            return File(result.Value.Content, "application/pdf", $"Invoice_{orderId}.pdf");
        }
        [Authorize(Roles = "Staff,Admin")]
        [HttpGet("export-excel")]
        [ProducesResponseType<FileContentResult>((int)HttpStatusCode.OK)]
        public async Task<IActionResult> ExportCsv([FromQuery] ExportOrdersToExcelRequest request)
        {
            var result = await _orderService.ExportOrdersToExcel(request);

            if (!result.IsSuccessful)
            {
                return result.Error switch
                {
                    ErrorCode.NotFound => NotFound(new ErrorResponse("No orders found in the specified date range",
                        ErrorType.ApiError, HttpStatusCode.NotFound, result.Error)),
                    _ => StatusCode(500,
                        new ErrorResponse("Error exporting orders", ErrorType.ApiError,
                            HttpStatusCode.InternalServerError, result.Error))
                };
            }

            return File(result.Value.Content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"Orders_{request.StartDate:yyyyMMdd}_{request.EndDate:yyyyMMdd}.xlsx");
        }

        [HttpGet("calculate-shipping-fee")]
        [ProducesResponseType<ShippingFeeResult>((int)HttpStatusCode.OK)]
        [ProducesResponseType<ErrorResponse>((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> CalculateShippingFee([FromQuery] List<Guid> itemIds,
            [FromQuery] int destinationDistrictId)
        {
            var result = await _orderService.CalculateShippingFee(itemIds, destinationDistrictId);

            if (!result.IsSuccessful)
            {
                return result.Error switch
                {
                    ErrorCode.ExternalServiceError => StatusCode(500,
                        new ErrorResponse("External Service Error", ErrorType.ApiError,
                            HttpStatusCode.InternalServerError, ErrorCode.ExternalServiceError)),
                    ErrorCode.UnsupportedShipping => StatusCode(400,
                        new ErrorResponse("Shipping is not supported for this address", ErrorType.ShippingError,
                            HttpStatusCode.BadRequest, ErrorCode.UnsupportedShipping)),
                    _ => StatusCode(500,
                        new ErrorResponse("Unexpected error from server", ErrorType.ApiError,
                            HttpStatusCode.InternalServerError, ErrorCode.ServerError))
                };
            }

            return Ok(result.Value);
        }

        [HttpPatch("{orderId}/checkout-auction")]
        [ProducesResponseType<PayWithPointsResponse>((int)HttpStatusCode.OK)]
        public async Task<IActionResult> CheckoutAuction([FromRoute] Guid orderId,
            [FromBody] CheckoutAuctionRequest request)
        {
            var result = await _orderService.CheckoutAuction(orderId, request);

            if (!result.IsSuccessful)
            {
                return result.Error switch
                {
                    ErrorCode.ExternalServiceError => StatusCode(500,
                        new ErrorResponse("External Service Error", ErrorType.ApiError,
                            HttpStatusCode.InternalServerError, ErrorCode.ExternalServiceError)),
                    ErrorCode.UnsupportedShipping => StatusCode(400,
                        new ErrorResponse("Shipping is not supported for this address", ErrorType.ShippingError,
                            HttpStatusCode.BadRequest, ErrorCode.UnsupportedShipping)),
                    _ => StatusCode(500,
                        new ErrorResponse("Unexpected error from server", ErrorType.ApiError,
                            HttpStatusCode.InternalServerError, ErrorCode.ServerError))
                };
            }

            return Ok(result.Value);
        }
    }
}