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

    [HttpPut("{withdrawId}/complete-request")]
    public async Task<ActionResult<CompleteWithdrawResponse>> CompleteWithdrawRequest([FromRoute] Guid withdrawId)
    {
        var result = await _withdrawService.CompleteWithdrawRequest(withdrawId);
        return Ok(result);
    }
}