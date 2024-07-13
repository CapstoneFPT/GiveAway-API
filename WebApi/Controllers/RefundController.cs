using BusinessObjects.Dtos.Commons;
using BusinessObjects.Dtos.Refunds;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NuGet.Protocol;
using Services.Refunds;

namespace WebApi.Controllers
{
    [Route("api/refunds")]
    [ApiController]
    public class RefundController : ControllerBase
    {
        private readonly IRefundService _refundService;

        public RefundController(IRefundService refundService)
        {
            _refundService = refundService;
        }

        [HttpGet("{refundId}")]
        public async Task<ActionResult<Result<RefundResponse>>> GetRefundById([FromRoute] Guid refundId)
        {
            return await _refundService.GetRefundById(refundId);
        }

        [HttpPut("{refundId}/approval")]
        public async Task<ActionResult<Result<RefundResponse>>> ApprovalRefundRequestFromShop([FromRoute] Guid refundId, RefundStatus refundStatus)
        {
            return await _refundService.ApprovalRefundRequestFromShop(refundId, refundStatus);
        }
    }
}
