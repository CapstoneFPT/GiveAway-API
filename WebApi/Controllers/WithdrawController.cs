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
   
   [HttpPut("{withdrawId}/approve")]
   public async Task<ActionResult<ApproveWithdrawResponse>> ApproveWithdraw([FromRoute] Guid withdrawId)
   {
      var result = await _withdrawService.ApproveWithdraw(withdrawId);
      return Ok(result);
   }
}