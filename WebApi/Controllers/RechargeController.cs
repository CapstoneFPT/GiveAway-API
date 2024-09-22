using System.ComponentModel.DataAnnotations;
using BusinessObjects.Dtos.Commons;
using BusinessObjects.Entities;
using Microsoft.AspNetCore.Mvc;
using Services.Recharges;
using Services.VnPayService;
using Services.Transactions;
using System.Net;
using BusinessObjects.Dtos.Recharges;
using Microsoft.AspNetCore.Authorization;

namespace WebApi.Controllers;
[Authorize]
[ApiController]
[Route("api/recharges")]
public class RechargeController : ControllerBase
{
    private readonly IRechargeService _rechargeService;

    public RechargeController(IRechargeService rechargeService
    )
    {
        _rechargeService = rechargeService;
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

    [Authorize(Roles = "Member")]
    [HttpPost("initiate")]
    [ProducesResponseType(typeof(RechargePurchaseResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> InitiateRecharge([FromBody] InitiateRechargeRequest request)
    {
        var result = await _rechargeService.InitiateRecharge(request);

        if (!result.IsSuccessful)
        {
            return StatusCode(500,
                new ErrorResponse("Error creating recharge", ErrorType.ApiError,
                    System.Net.HttpStatusCode.InternalServerError, result.Error));
        }

        return Ok(result.Value);
    }
    [AllowAnonymous]
    [HttpGet("payment-return")]
    public async Task<IActionResult> PaymentReturn()
    {
        var requestParams = Request.Query;
        var result = await _rechargeService.ProcessPaymentReturn(requestParams);

        if (!result.IsSuccessful)
        {
            return StatusCode(500,
                new ErrorResponse("Error processing payment return", ErrorType.ApiError,
                    System.Net.HttpStatusCode.InternalServerError, result.Error));
        }

        return Redirect(result.Value);
    }
}