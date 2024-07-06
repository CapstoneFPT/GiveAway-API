using BusinessObjects.Dtos.Commons;
using BusinessObjects.Dtos.Orders;
using BusinessObjects.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Services.Orders;
using Services.PointPackages;
using Services.Transactions;
using Services.VnPayService;

namespace WebApi.Controllers;

[ApiController]
[Route("api/pointpackages")]
public class PointPackageController : ControllerBase
{
    private readonly ILogger<PointPackageController> _logger;
    private readonly IPointPackageService _pointPackageService;
    private readonly IOrderService _orderService;
    private readonly IVnPayService _vnPayService;
    private readonly ITransactionService _transactionService;

    public PointPackageController(ILogger<PointPackageController> logger, IPointPackageService pointPackageService,
         IOrderService orderService,IVnPayService vnPayService,ITransactionService transactionService)
    {
        _logger = logger;
        _pointPackageService = pointPackageService;
        _orderService = orderService;
        _vnPayService = vnPayService;
        _transactionService = transactionService;
    }

    [HttpGet()]
    public async Task<IActionResult> Get()
    {
        var result = await _pointPackageService.GetList();
        return Ok(result);
    }

    [HttpGet("{pointPackageId}")]
    public async Task<IActionResult> Get(Guid pointPackageId)
    {
        var result = await _pointPackageService.GetPointPackageDetail(pointPackageId);
        return Ok(result);
    }

  [HttpPost("{pointPackageId}/purchase")]
    public async Task<IActionResult> Purchase([FromRoute] Guid pointPackageId, [FromBody] PurchasePointPackageRequest request)
    {
        try
        {
            var pointPackage = await _pointPackageService.GetPointPackageDetail(pointPackageId);
            if (pointPackage == null)
            {
                return NotFound("Point package not found");
            }

            var order = new PointPackageOrder() 
            {
                MemberId = request.MemberId,
                TotalPrice = pointPackage.Price,
                Status = OrderStatus.AwaitingPayment,
                CreatedDate = DateTime.UtcNow,
                PaymentMethod = PaymentMethod.QRCode,
                PointPackageId = pointPackageId
            };

            var orderResult = await _orderService.CreatePointPackageOrder(order);

            var paymentUrl = _vnPayService.CreatePaymentUrl(
                orderResult.Data
                    .OrderCode,
                orderResult.Data
                    .TotalPrice,
                $"Purchase point package: {pointPackage.Points} points"
            );

            _logger.LogInformation($"Point package purchase initiated. OrderCode: {orderResult.Data.OrderCode}, MemberId: {request.MemberId}, Package: {pointPackage.Points} points");

            return Ok(new { paymentUrl, orderCode = order.OrderCode });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error initiating point package purchase");
            return StatusCode(500, "An error occurred while processing your request.");
        }
    }

    [HttpGet("payment-return")]
    public async Task<IActionResult> PaymentReturn([FromQuery] IQueryCollection collection)
    {
        var response = _vnPayService.ProcessPayment(collection);

        if (response.Success)
        {
            try
            {
                var transaction = await _transactionService.CreateTransaction(response);
                var order = await _orderService.GetOrderByCode(response.OrderId);

                if (order == null)
                {
                    _logger.LogWarning($"Order not found for OrderCode: {response.OrderId}");
                    return BadRequest(new { success = false, message = "Order not found" });
                }

                if (order.Status != OrderStatus.AwaitingPayment)
                {
                    _logger.LogWarning($"Order already processed: {response.OrderId}");
                    return Ok(new { success = true, message = "Order already processed", orderCode = response.OrderId });
                }

                await _pointPackageService.AddPointsToBalance(order.MemberId, 4444);

                order.Status = OrderStatus.Completed;
                order.PaymentDate = DateTime.UtcNow;
                await _orderService.UpdateOrder(order);

                _logger.LogInformation($"Point package purchase successful. OrderCode: {response.OrderId}, Points: {order.TotalPrice}");

                return Ok(new { success = true, message = "Payment successful", orderCode = response.OrderId, points = order.TotalPrice });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing successful payment");
                return StatusCode(500, new { success = false, message = "An error occurred while processing your payment." });
            }
        }

        _logger.LogWarning($"Payment failed. OrderCode: {response.OrderId}, ResponseCode: {response.VnPayResponseCode}");
        return Ok(new { success = false, message = "Payment failed", orderCode = response.OrderId });
    }
}

public class PurchasePointPackageRequest
{
    public Guid MemberId { get; set; }
}

public class VnPayResponseData
{
    public string vnp_TmnCode { get; set; }
    public string vnp_Amount { get; set; }
    public string vnp_BankCode { get; set; }
    public string vnp_BankTranNo { get; set; }
    public string vnp_CardType { get; set; }
    public string vnp_PayDate { get; set; }
    public string vnp_OrderInfo { get; set; }
    public string vnp_TransactionNo { get; set; }
    public string vnp_ResponseCode { get; set; }
    public string vnp_TransactionStatus { get; set; }
    public string vnp_TxnRef { get; set; }
    public string vnp_SecureHashType { get; set; }
    public string vnp_SecureHash { get; set; }
}