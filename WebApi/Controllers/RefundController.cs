using System.Net;
using BusinessObjects.Dtos.Commons;
using BusinessObjects.Dtos.Refunds;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NuGet.Protocol;
using Services.OrderDetails;
using Services.Refunds;

namespace WebApi.Controllers
{
    [Route("api/refunds")]
    [ApiController]
    [EnableCors("AllowAll")]
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
            var result = await _refundService.GetRefundById(refundId);

            if (result.ResultStatus != ResultStatus.Success)
                return StatusCode((int)HttpStatusCode.InternalServerError, result);

            return Ok(result);
        }

        [HttpPut("{refundId}/approval")]
        public async Task<ActionResult<Result<RefundResponse>>> ApprovalRefundRequestFromShop([FromRoute] Guid refundId,
            [FromBody] ApprovalRefundRequest request)
        {
            var result = await _refundService.ApprovalRefundRequestFromShop(refundId, request);

            if (result.ResultStatus != ResultStatus.Success)
                return StatusCode((int)HttpStatusCode.InternalServerError, result);

            return Ok(result);
        }

        [HttpPut("{refundId}/confirm-received-and-refund")]
        public async Task<ActionResult<Result<RefundResponse>>> ConfirmReceivedAndRefund([FromRoute] Guid refundId)
        {
            var result = await _refundService.ConfirmReceivedAndRefund(refundId);
            if (result.ResultStatus != ResultStatus.Success)
                return StatusCode((int)HttpStatusCode.InternalServerError, result);
            return Ok(result);
        }

        [HttpPost]
        public async Task<ActionResult<Result<RefundResponse>>> RequestRefundItemToShop(
            [FromBody] CreateRefundRequest refundRequest)
        {
            var result = await _orderDetailService.RequestRefundToShop(refundRequest);

            if (result.ResultStatus != ResultStatus.Success)
                return StatusCode((int)HttpStatusCode.InternalServerError, result);

            return Ok(result);
        }

        [HttpGet]
        public async Task<ActionResult<Result<PaginationResponse<RefundResponse>>>> GetAllRefunds(
            [FromQuery] RefundRequest refundRequest)
        {
            var result = await _refundService.GetAllRefunds(refundRequest);

            if (result.ResultStatus != ResultStatus.Success)
                return StatusCode((int)HttpStatusCode.InternalServerError, result);

            return Ok(result);
        }
    }
}