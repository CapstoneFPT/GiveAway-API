using System.Transactions;
using BusinessObjects.Dtos.Commons;
using BusinessObjects.Dtos.FashionItems;
using BusinessObjects.Dtos.OrderDetails;
using BusinessObjects.Dtos.Orders;
using BusinessObjects.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Services.Accounts;
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

        public OrderController(IOrderService orderService, IOrderDetailService orderDetailService, IVnPayService
                vnPayService, ITransactionService transactionService, ILogger<OrderController> logger,
            IAccountService accountService)
        {
            _orderService = orderService;
            _orderDetailService = orderDetailService;
            _vnPayService = vnPayService;
            _transactionService = transactionService;
            _accountService = accountService;
            _logger = logger;
        }

        [HttpGet("{OrderId}/orderdetails")]
        public async Task<ActionResult<Result<PaginationResponse<OrderDetailResponse<FashionItem>>>>>
            GetOrderDetailsByOrderId([FromRoute] Guid OrderId, [FromQuery] OrderDetailRequest request)
        {
            return await _orderDetailService.GetOrderDetailsByOrderId(OrderId, request);
        }

        [HttpGet("{OrderId}/orderdetails/{OrderdetailId}")]
        public async Task<ActionResult<Result<OrderDetailResponse<FashionItem>>>> GetOrderDetailById(
            [FromRoute] Guid OrderdetailId)
        {
            return await _orderDetailService.GetOrderDetailById(OrderdetailId);
        }

        [HttpPut("{OrderId}/cancel")]
        public async Task<ActionResult<Result<string>>> CancelOrder([FromRoute] Guid OrderId)
        {
            return await _orderService.CancelOrder(OrderId);
        }

        [HttpPost("{orderId}/pay/vnpay")]
        public async Task<IActionResult> PurchaseOrder([FromRoute] Guid orderId,
            [FromBody] PurchaseOrderRequest request)
        {
            var order = await _orderService.GetOrderById(orderId);

            if (order == null)
            {
                throw new Exception("Order not found");
            }

            if (order.PaymentMethod != PaymentMethod.QRCode)
            {
                throw new Exception("Order is not paid by QR code");
            }

            var paymentUrl = _vnPayService.CreatePaymentUrl(
                order.OrderId,
                order.TotalPrice,
                $"{orderId}", "orders");

            return Ok(new { paymentUrl });
        }

        [HttpGet("payment-return")]
        public async Task<IActionResult> PaymentReturn()
        {
            var requestParams = Request.Query;
            var response = _vnPayService.ProcessPayment(requestParams);
            var order = await _orderService.GetOrderById(new Guid(response.OrderId));
            if (response.Success)
            {
                try
                {
                    if (order == null)
                    {
                        _logger.LogWarning($"Order not found for OrderCode: {response.OrderId}");
                        return BadRequest(new { success = false, message = "Order not found" });
                    }

                    if (order.Status != OrderStatus.AwaitingPayment)
                    {
                        _logger.LogWarning($"Order already processed: {response.OrderId}");
                        return Ok(new
                            { success = true, message = "Order already processed", orderCode = response.OrderId });
                    }

                    var transaction = await _transactionService.CreateTransaction(response, TransactionType.Purchase);

                    if (transaction.ResultStatus == ResultStatus.Success)
                    {
                        order.Status = OrderStatus.Completed;
                        order.PaymentDate = DateTime.UtcNow;

                        await _orderService.UpdateOrder(order);
                        await _orderService.UpdateFashionItemStatus(order.OrderId);
                        await _orderService.UpdateShopBalance(order);

                        return Ok(new
                            { success = true, message = "Payment success", orderCode = response.OrderId });
                    }
                }
                catch (Exception e)
                {
                    _logger.LogError(e.Message);
                    return StatusCode(500, new { success = false, message = "Payment failed" });
                }
            }

            _logger.LogWarning(
                $"Payment failed. OrderCode: {response.OrderId}, ResponseCode: {response.VnPayResponseCode}");
            return Ok(new { success = false, message = "Payment failed", orderCode = response.OrderId });
        }

        [HttpPost("{orderId}/pay/points")]
        public async Task<IActionResult> PurchaseOrderWithPoints([FromRoute] Guid orderId,
            [FromBody] PurchaseOrderRequest request)
        {
            var order = await _orderService.GetOrderById(orderId);

            if (order == null)
            {
                throw new Exception("Order");
            }

            if (order.PaymentMethod != PaymentMethod.Point)
            {
                throw new Exception("Order is not paid by points");
            }


            await _accountService.DeductPoints(request.MemberId, order.TotalPrice);
            await _orderService.UpdateOrder(order);
            await _orderService.UpdateFashionItemStatus(order.OrderId);
            await _orderService.UpdateShopBalance(order);
            
            return Ok(new
                { success = true, message = "Payment success", orderCode = order.OrderId });
        }
    }

    public class PurchaseOrderRequest
    {
        public Guid MemberId { get; set; }
    }
}