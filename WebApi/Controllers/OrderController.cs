﻿using System.Net;
using BusinessObjects.Dtos.AuctionDeposits;
using BusinessObjects.Dtos.Commons;
using BusinessObjects.Dtos.OrderLineItems;
using BusinessObjects.Dtos.Orders;
using BusinessObjects.Entities;
using BusinessObjects.Utils;
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
    [Route("api/orders")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _orderService;
        private readonly IOrderLineItemService _orderLineItemService;
        private readonly IVnPayService _vnPayService;
        private readonly ITransactionService _transactionService;
        private readonly ILogger<OrderController> _logger;
        private readonly IEmailService _emailService;
        private readonly IConsignSaleService _consignSaleService;

        public OrderController(IOrderService orderService, IOrderLineItemService orderLineItemService, IVnPayService
                vnPayService, ITransactionService transactionService, ILogger<OrderController> logger,
            IEmailService emailService, IConsignSaleService consignSaleService)
        {
            _orderService = orderService;
            _orderLineItemService = orderLineItemService;
            _vnPayService = vnPayService;
            _transactionService = transactionService;
            _emailService = emailService;
            _logger = logger;
            _consignSaleService = consignSaleService;
        }

        [HttpGet]
        [ProducesResponseType<PaginationResponse<OrderListResponse>>((int) HttpStatusCode.OK)]
        [ProducesResponseType<ErrorResponse>((int) HttpStatusCode.InternalServerError)]
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
        [ProducesResponseType<PaginationResponse<OrderLineItemListResponse>>((int) HttpStatusCode.OK)]
        [ProducesResponseType<ErrorResponse>((int) HttpStatusCode.InternalServerError)]
        public async Task<IActionResult>
            GetOrderLineItemByOrderId([FromRoute] Guid orderId, [FromQuery] OrderLineItemRequest request)
        {
            var result = await _orderService.GetOrderLineItemByOrderId(orderId,request);

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
        [ProducesResponseType<OrderDetailedResponse>((int) HttpStatusCode.OK)]
        [ProducesResponseType<ErrorResponse>((int)HttpStatusCode.InternalServerError)]
        [ProducesResponseType<ErrorResponse>((int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> GetOrderById([FromRoute] Guid orderId)
        {
            var result = await _orderService.GetDetailedOrder(orderId);

            if (!result.IsSuccessful)
            {
                return result.Error switch
                {
                    ErrorCode.NotFound => NotFound(new ErrorResponse("Order not found", ErrorType.ApiError, HttpStatusCode.NotFound, result.Error)),
                    _ => StatusCode(500,
                        new ErrorResponse("Error fetching order", ErrorType.ApiError,
                            HttpStatusCode.InternalServerError, result.Error))
                };
            }

            return Ok(result.Value);
        }

        

        [HttpPut("{OrderId}/cancel")]
        public async Task<ActionResult<Result<string>>> CancelOrder([FromRoute] Guid OrderId)
        {
            var result = await _orderService.CancelOrder(OrderId);

            if (result.ResultStatus != ResultStatus.Success)
                return StatusCode((int)HttpStatusCode.InternalServerError, result);

            return Ok(result);
        }

        

        [HttpPost("{orderId}/pay/vnpay")]
        public async Task<ActionResult<VnPayPurchaseResponse>> PurchaseOrder([FromRoute] Guid orderId,
            [FromBody] PurchaseOrderRequest request)
        {
            var order = await _orderService.GetOrderById(orderId);

            if (order == null)
            {
                throw new OrderNotFoundException();
            }

            if (order.PaymentMethod != PaymentMethod.QRCode)
            {
                throw new WrongPaymentMethodException("Order is not paid by QRCode");
            }

            if (order.Status != OrderStatus.AwaitingPayment)
            {
                throw new InvalidOperationException("Order is not awaiting payment");
            }

            if (order.MemberId != request.MemberId)
            {
                throw new NotAuthorizedToPayOrderException();
            }

            //check

            var paymentUrl = _vnPayService.CreatePaymentUrl(
                order.OrderId,
                order.TotalPrice,
                $"{orderId}", "orders");

            return Ok(new VnPayPurchaseResponse { PaymentUrl = paymentUrl });
        }

        [HttpGet("payment-return")]
        public async Task<IActionResult> PaymentReturn()
        {
            var requestParams = Request.Query;
            //check
            var response = _vnPayService.ProcessPayment(requestParams);
            var order = await _orderService.GetOrderById(new Guid(response.OrderId));

            if (order.Status != OrderStatus.AwaitingPayment)
            {
                throw new InvalidOperationException("Order is not awaiting payment");
            }

            if (response.Success)
            {
                try
                {
                    if (order == null)
                    {
                        _logger.LogWarning("Order not found for OrderCode: {OrderId}", response.OrderId);
                        return BadRequest(new { success = false, message = "Order not found" });
                    }

                    if (order.Status != OrderStatus.AwaitingPayment)
                    {
                        _logger.LogWarning("Order already processed: {OrderId}", response.OrderId);
                        return Ok(new
                            { success = true, message = "Order already processed", orderCode = response.OrderId });
                    }

                    var transaction =
                        await _transactionService.CreateTransactionFromVnPay(response, TransactionType.Purchase);

                    if (transaction.ResultStatus == ResultStatus.Success)
                    {
                        order.Status = OrderStatus.Pending;
                        foreach (var orderDetail in order.OrderLineItems)
                        {
                            orderDetail.PaymentDate = DateTime.UtcNow;
                        }

                        await _orderService.UpdateOrder(order);
                        await _orderService.UpdateFashionItemStatus(order.OrderId);
                        await _orderService.UpdateAdminBalance(order);
                        await _emailService.SendEmailOrder(order);

                        return Redirect("https://giveawayproject.jettonetto.org");
                    }
                }
                catch (Exception e)
                {
                    _logger.LogError(e, e.Message);
                    return StatusCode(500, new { success = false, message = "Payment failed" });
                }
            }

            _logger.LogWarning(
                "Payment failed. OrderCode: {OrderId}, ResponseCode: {VnPayResponseCode}", response.OrderId,
                response.VnPayResponseCode);
            return Ok(new { success = false, message = "Payment failed", orderCode = response.OrderId });
        }

        [HttpPost("{orderId}/pay/points")]
        public async Task<ActionResult<PayWithPointsResponse>> PurchaseOrderWithPoints([FromRoute] Guid orderId,
            [FromBody] PurchaseOrderRequest request)
        {
            var order = await _orderService.GetOrderById(orderId);

            if (order == null)
            {
                throw new OrderNotFoundException();
            }

            if (order.PaymentMethod != PaymentMethod.Point)
            {
                throw new WrongPaymentMethodException("Order is not paid by Point");
            }

            if (order.Status != OrderStatus.AwaitingPayment)
            {
                throw new InvalidOperationException("Order is not awaiting payment");
            }

            if (order.MemberId != request.MemberId)
            {
                throw new NotAuthorizedToPayOrderException();
            }

            foreach (var orderDetail in order.OrderLineItems)
            {
                orderDetail.PaymentDate = DateTime.UtcNow;
            }
            order.Status = OrderStatus.Pending;
            order.Member!.Balance -= order.TotalPrice;

            await _transactionService.CreateTransactionFromPoints(order, request.MemberId, TransactionType.Purchase);
            await _orderService.UpdateOrder(order);
            await _orderService.UpdateFashionItemStatus(order.OrderId);
            await _orderService.UpdateAdminBalance(order);
            await _consignSaleService.UpdateConsignPrice(order.OrderId);
            await _emailService.SendEmailOrder(order);

            return Ok(new PayWithPointsResponse()
                { Sucess = true, Message = "Payment success", OrderId = order.OrderId });
        }

      

        [HttpPut("{orderId}/cancelbyadmin")]
        public async Task<ActionResult<Result<string>>> CancelOrderByAdmin([FromRoute] Guid orderId)
        {
            var result = await _orderService.CancelOrderByAdmin(orderId);

            if (result.ResultStatus != ResultStatus.Success)
                return StatusCode((int)HttpStatusCode.InternalServerError, result);

            return Ok(result);
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
                    ErrorCode.UnsupportedShipping => StatusCode(400, new ErrorResponse("Shipping is not supported for this address",ErrorType.ShippingError,HttpStatusCode.BadRequest,ErrorCode.UnsupportedShipping)),
                    _ => StatusCode(500,
                        new ErrorResponse("Unexpected error from server", ErrorType.ApiError,
                            HttpStatusCode.InternalServerError, ErrorCode.ServerError))
                };
            }

            return Ok(result.Value);
        }
    }

    public class VnPayPurchaseResponse
    {
        public string PaymentUrl { get; set; }
    }

    public class PayWithPointsResponse
    {
        public bool Sucess { get; set; }
        public string Message { get; set; }
        public Guid OrderId { get; set; }
    }


    public class PurchaseOrderRequest
    {
        public Guid MemberId { get; set; }
    }
}