using BusinessObjects.Dtos.Commons;
using BusinessObjects.Entities;
using Microsoft.AspNetCore.Mvc;
using Services.Recharges;
using Services.VnPayService;
using Services.Transactions;
using System.Net;
using BusinessObjects.Dtos.Recharges;

namespace WebApi.Controllers;

[ApiController]
[Route("api/recharges")]
public class RechargeController : ControllerBase
{
    private readonly ILogger<RechargeController> _logger;
    private readonly IRechargeService _rechargeService;
    private readonly IVnPayService _vnPayService;
    private readonly ITransactionService _transactionService;

    public RechargeController(ILogger<RechargeController> logger, IRechargeService rechargeService,
        IVnPayService vnPayService, ITransactionService transactionService)
    {
        _logger = logger;
        _rechargeService = rechargeService;
        _vnPayService = vnPayService;
        _transactionService = transactionService;
    }

    [HttpGet]
    [ProducesResponseType<PaginationResponse<RechargeListResponse>>((int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.InternalServerError)]
    public async Task<IActionResult> GetRecharges([FromQuery] GetRechargesRequest paginationRequest)
    {
        var result = await _rechargeService.GetRecharges(paginationRequest);

        if (!result.IsSuccessful)
        {
            return StatusCode((int)HttpStatusCode.InternalServerError,
                new ErrorResponse("Error getting recharges", ErrorType.ApiError, HttpStatusCode.InternalServerError,
                    result.Error));
        }

        return Ok(result.Value);
    }


    [HttpPost("initiate")]
    [ProducesResponseType(typeof(RechargePurchaseResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> InitiateRecharge([FromBody] InitiateRechargeRequest request)
    {
        var recharge = new Recharge
        {
            MemberId = request.MemberId,
            Amount = request.Amount,
            Status = RechargeStatus.Pending,
            CreatedDate = DateTime.UtcNow,
            PaymentMethod = PaymentMethod.QRCode
        };

        var rechargeResult = await _rechargeService.CreateRecharge(recharge);

        if (!rechargeResult.IsSuccessful)
        {
            return StatusCode(500,
                new ErrorResponse("Error creating recharge", ErrorType.ApiError,
                    System.Net.HttpStatusCode.InternalServerError, rechargeResult.Error));
        }

        var paymentUrl = _vnPayService.CreatePaymentUrl(
            rechargeResult.Value.RechargeId,
            rechargeResult.Value.Amount,
            $"Recharge account: {rechargeResult.Value.Amount} VND",
            "recharges");

        _logger.LogInformation(
            "Recharge initiated. RechargeId: {RechargeId}, MemberId: {MemberId}, Amount: {Amount} VND",
            rechargeResult.Value.RechargeId, request.MemberId, request.Amount);

        return Ok(new RechargePurchaseResponse
        {
            PaymentUrl = paymentUrl,
            RechargeId = rechargeResult.Value.RechargeId
        });
    }

    [HttpGet("payment-return")]
    public async Task<IActionResult> PaymentReturn()
    {
        var requestParams = Request.Query;
        var response = _vnPayService.ProcessPayment(requestParams);
        var redirectUrl = "https://giveawayproject.jettonetto.org/process-payment";

        if (response.Success)
        {
            try
            {
                var rechargeId = new Guid(response.OrderId);
                var rechargeResult = await _rechargeService.GetRechargeById(rechargeId);

                if (!rechargeResult.IsSuccessful)
                {
                    return Redirect(
                        $"{redirectUrl}?paymentstatus=error&message={Uri.EscapeDataString(rechargeResult.Error.ToString())}");
                }

                var recharge = rechargeResult.Value;

                if (recharge.Status != RechargeStatus.Pending)
                {
                    _logger.LogWarning("Recharge already processed: {RechargeId}", response.OrderId);
                    return Redirect(
                        $"{redirectUrl}?paymentstatus=warning&message={Uri.EscapeDataString("Recharge already processed")}");
                }

                await _transactionService.CreateTransactionFromVnPay(response, TransactionType.Recharge);
                var completeResult = await _rechargeService.CompleteRecharge(recharge.RechargeId, recharge.Amount);

                if (!completeResult.IsSuccessful)
                {
                    return Redirect(
                        $"{redirectUrl}?paymentstatus=error&message={Uri.EscapeDataString("Error completing recharge")}");
                }

                _logger.LogInformation(
                    "Recharge successful. RechargeId: {RechargeId}, Amount: {Amount}", response.OrderId,
                    recharge.Amount);

                return Redirect($"{redirectUrl}?payementstatus=success&message={Uri.EscapeDataString("Recharge successful")}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing successful payment");
                return Redirect(
                    $"{redirectUrl}?paymentstatus=error&message={Uri.EscapeDataString("An error occurred while processing your payment")}");
            }
        }
        else
        {
            try
            {
                var failResult = await _rechargeService.FailRecharge(new Guid(response.OrderId));
                if (!failResult.IsSuccessful)
                {
                    _logger.LogError("Failed to mark recharge as failed. RechargeId: {RechargeId}", response.OrderId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking recharge as failed. RechargeId: {RechargeId}", response.OrderId);
            }

            _logger.LogWarning(
                "Payment failed. RechargeId: {RechargeId}, ResponseCode: {VnPayResponseCode}", response.OrderId,
                response.VnPayResponseCode);
            return Redirect($"{redirectUrl}?paymentstatus=error&message={Uri.EscapeDataString("Payment failed")}");
        }
    }
}

public class RechargePurchaseResponse
{
    public required string PaymentUrl { get; set; }
    public Guid RechargeId { get; set; }
}

public class InitiateRechargeRequest
{
    public Guid MemberId { get; set; }
    public decimal Amount { get; set; }
}