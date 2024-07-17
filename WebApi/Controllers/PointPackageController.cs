using System.Collections.Specialized;
using BusinessObjects.Dtos.Commons;
using BusinessObjects.Dtos.Orders;
using BusinessObjects.Dtos.PointPackages;
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
        IOrderService orderService, IVnPayService vnPayService, ITransactionService transactionService)
    {
        _logger = logger;
        _pointPackageService = pointPackageService;
        _orderService = orderService;
        _vnPayService = vnPayService;
        _transactionService = transactionService;
    }

    [HttpGet()]
    public async Task<ActionResult<PaginationResponse<PointPackageListResponse>>> GetPointPackages(
        [FromQuery] GetPointPackagesRequest request)
    {
        PaginationResponse<PointPackageListResponse> result = await _pointPackageService.GetList(request);
        return Ok(result);
    }

    [HttpGet("{pointPackageId}")]
    public async Task<ActionResult<PointPackageDetailResponse>> Get(Guid pointPackageId)
    {
        var result = await _pointPackageService.GetPointPackageDetail(pointPackageId);
        return Ok(result);
    }

    [HttpPost("{pointPackageId}/purchase")]
    public async Task<IActionResult> Purchase([FromRoute] Guid pointPackageId,
        [FromBody] PurchasePointPackageRequest request)
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
                .OrderId,
            orderResult.Data
                .TotalPrice,
            $"Purchase point package: {pointPackage.Points} points", "pointpackages");

        _logger.LogInformation(
            "Point package purchase initiated. OrderCode: {OrderCode}, MemberId: {MemberId}, Package: {Points} points",
            orderResult.Data.OrderCode, request.MemberId, pointPackage.Points);

        return Ok(new { paymentUrl, orderCode = orderResult.Data.OrderCode });
    }

    [HttpGet("payment-return")]
    public async Task<IActionResult> PaymentReturn()
    {
        var requestParams = Request.Query;
        var response = _vnPayService.ProcessPayment(requestParams);

        if (response.Success)
        {
            try
            {
                await _transactionService.CreateTransactionFromVnPay(response, TransactionType.Recharge);
                var order = await _orderService.GetOrderById(new Guid(response.OrderId));
                var orderDetails = await _orderService.GetOrderDetailByOrderId(new Guid(response.OrderId));
                var pointPackageId = orderDetails[0].PointPackageId!.Value;
                var pointPackage = await _pointPackageService.GetPointPackageDetail(pointPackageId);

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

                await _pointPackageService.AddPointsToBalance(order.MemberId!.Value, amount: pointPackage!.Points);

                order.Status = OrderStatus.Completed;
                order.PaymentDate = DateTime.UtcNow;
                await _orderService.UpdateOrder(order);

                _logger.LogInformation(
                    "Point package purchase successful. OrderCode: {OrderId}, Points: {TotalPrice}", response.OrderId,
                    order.TotalPrice);

                return Ok(new
                {
                    success = true, message = "Payment successful", orderCode = response.OrderId,
                    TotalPrice = order.TotalPrice
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing successful payment");
                return StatusCode(500,
                    new { success = false, message = "An error occurred while processing your payment." });
            }
        }

        _logger.LogWarning(
            "Payment failed. OrderCode: {OrderId}, ResponseCode: {VnPayResponseCode}", response.OrderId,
            response.VnPayResponseCode);
        return Ok(new  { success = false, message = "Payment failed", orderCode = response.OrderId });
    }
}

public class PurchasePointPackageRequest
{
    public Guid MemberId { get; set; }
}