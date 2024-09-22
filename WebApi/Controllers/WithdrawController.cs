using System.Net;
using BusinessObjects.Dtos.Commons;
using BusinessObjects.Dtos.Withdraws;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Services.Withdraws;

namespace WebApi.Controllers;

[Route("api/withdraws")]
[ApiController]
public class WithdrawController : ControllerBase
{
    private readonly IWithdrawService _withdrawService;

    public WithdrawController(IWithdrawService withdrawService)
    {
        _withdrawService = withdrawService;
    }
    [Authorize(Roles = "Admin")]
    [HttpPut("{withdrawId}/complete-request")]
    public async Task<ActionResult<CompleteWithdrawResponse>> CompleteWithdrawRequest([FromRoute] Guid withdrawId)
    {
        var result = await _withdrawService.CompleteWithdrawRequest(withdrawId);
        return Ok(result);
    }
    [Authorize]
    [HttpGet]
    [ProducesResponseType<PaginationResponse<GetWithdrawsResponse>>((int)HttpStatusCode.OK)]
    [ProducesResponseType<ErrorResponse>((int)HttpStatusCode.InternalServerError)]
    public async Task<IActionResult> GetAllPaginationWithdraws([FromQuery] GetWithdrawByAdminRequest request)
    {
        var result = await _withdrawService.GetAllPaginationWithdraws(request);
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
}