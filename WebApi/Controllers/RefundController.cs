using BusinessObjects.Dtos.Commons;
using BusinessObjects.Dtos.Refunds;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NuGet.Protocol;
using Services.OrderDetails;
using Services.Refunds;

namespace WebApi.Controllers
{
    [Route("api/refunds")]
    [ApiController]
    public class RefundController : ControllerBase
    {
        private readonly IRefundService _refundService;
        private readonly IOrderDetailService _orderDetailService;

        public RefundController(IRefundService refundService, IOrderDetailService orderDetailService)
        {
            _refundService = refundService;
            _orderDetailService = orderDetailService;
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
        [HttpPost]
        public async Task<ActionResult<Result<List<RefundResponse>>>> RequestRefundItemToShop([FromBody] List<CreateRefundRequest> refundRequest)
        {
            return await _orderDetailService.RequestRefundToShop(refundRequest);
        }
    }
}
