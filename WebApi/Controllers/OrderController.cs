﻿using System.Net;
using BusinessObjects.Dtos.Commons;
using BusinessObjects.Dtos.OrderDetails;
using BusinessObjects.Dtos.Orders;
using BusinessObjects.Entities;
using BusinessObjects.Utils;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Services.Accounts;
using Services.ConsignSales;
using Services.Emails;
using Services.OrderDetails;
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
        private readonly IOrderDetailService _orderDetailService;
        private readonly IVnPayService _vnPayService;
        private readonly ITransactionService _transactionService;
        private readonly ILogger<OrderController> _logger;
        private readonly IAccountService _accountService;
        private readonly IEmailService _emailService;
        private readonly IConsignSaleService _consignSaleService;
        public OrderController(IOrderService orderService, IOrderDetailService orderDetailService, IVnPayService
                vnPayService, ITransactionService transactionService, ILogger<OrderController> logger,
            IAccountService accountService, IEmailService emailService, IConsignSaleService consignSaleService)
        {
            _orderService = orderService;
            _orderDetailService = orderDetailService;
            _vnPayService = vnPayService;
            _transactionService = transactionService;
            _accountService = accountService;
            _emailService = emailService;
            _logger = logger;
            _consignSaleService = consignSaleService;
        }

        [HttpGet]
        public async Task<ActionResult<Result<PaginationResponse<OrderResponse>>>> GetOrders(
            [FromQuery] OrderRequest orderRequest)
        {
            var result = await _orderService.GetOrders(orderRequest);

            if (result.ResultStatus != ResultStatus.Success)
                return StatusCode((int)HttpStatusCode.InternalServerError, result);

            return Ok(result);
        }

        [HttpGet("{OrderId}/orderdetails")]
        public async Task<ActionResult<Result<PaginationResponse<OrderDetailsResponse>>>>
            GetOrderDetailsByOrderId([FromRoute] Guid OrderId, [FromQuery] OrderDetailRequest request)
        {
            return await _orderDetailService.GetOrderDetailsByOrderId(OrderId, request);
        }
        

        [HttpGet("orderdetails/{OrderdetailId}")]
        public async Task<ActionResult<Result<OrderDetailResponse<FashionItem>>>> GetOrderDetailById(
            [FromRoute] Guid OrderdetailId)
        {
            var result = await _orderDetailService.GetOrderDetailById(OrderdetailId);

            if (result.ResultStatus != ResultStatus.Success)
                return StatusCode((int)HttpStatusCode.InternalServerError, result);


            return Ok(result);
        }

        [HttpPut("{OrderId}/cancel")]
        public async Task<ActionResult<Result<string>>> CancelOrder([FromRoute] Guid OrderId)
        {
            var result = await _orderService.CancelOrder(OrderId);

            if (result.ResultStatus != ResultStatus.Success)
                return StatusCode((int)HttpStatusCode.InternalServerError, result);

            return Ok(result);
        }

        [HttpPut("{OrderId}/confirm-deliveried")]
        public async Task<ActionResult<Result<OrderResponse>>> ConfirmOrderDelivered(
            [FromRoute] Guid OrderId)
        {
            var result = await _orderService.ConfirmOrderDeliveried(OrderId);

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
                        order.PaymentDate = DateTime.UtcNow;

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

            order.PaymentDate = DateTime.UtcNow;
            order.Status = OrderStatus.Pending;
            order.Member.Balance -= order.TotalPrice;

            await _transactionService.CreateTransactionFromPoints(order, request.MemberId, TransactionType.Purchase);
            await _orderService.UpdateOrder(order);
            await _orderService.UpdateFashionItemStatus(order.OrderId);
            await _orderService.UpdateAdminBalance(order);
            await _consignSaleService.UpdateConsignPrice(order.OrderId);
            await _emailService.SendEmailOrder(order);

            return Ok(new PayWithPointsResponse()
                { Sucess = true, Message = "Payment success", OrderId = order.OrderId });
        }

        [HttpPut("{orderId}/orderdetails/{orderdetailId}/confirm-pending-order")]
        public async Task<ActionResult<Result<OrderResponse>>> ConfirmPendingOrderDetailByShop([FromRoute] Guid orderId, [FromRoute] Guid orderdetailId)
        {
            var result = await _orderService.ConfirmPendingOrder(orderId, orderdetailId);
            return Ok(result);
        }
        [HttpPut("{orderId}/cancelbyadmin")]
        public async Task<ActionResult<Result<string>>> CancelOrderByAdmin([FromRoute] Guid orderId)
        {
            var result = await _orderService.CancelOrderByAdmin(orderId);

            if (result.ResultStatus != ResultStatus.Success)
                return StatusCode((int)HttpStatusCode.InternalServerError, result);

            return Ok(result);
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