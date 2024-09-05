using System.Net;
using BusinessObjects.Dtos.Commons;
using BusinessObjects.Dtos.Refunds;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NuGet.Protocol;
using Services.OrderLineItems;
using Services.Refunds;

namespace WebApi.Controllers
{
    [Route("api/refunds")]
    [ApiController]
    public class RefundController : ControllerBase
    {
        private readonly IRefundService _refundService;
        private readonly IOrderLineItemService _orderLineItemService;

        public RefundController(IRefundService refundService, IOrderLineItemService orderLineItemService)
        {
            _refundService = refundService;
            _orderLineItemService = orderLineItemService;
        }

        [HttpGet("{refundId}")]
        [ProducesResponseType<RefundResponse>((int)HttpStatusCode.OK)]
        [ProducesResponseType<ErrorResponse>((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetRefundById([FromRoute] Guid refundId)
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
            var result = await _orderLineItemService.RequestRefundToShop(refundRequest);

            if (result.ResultStatus != ResultStatus.Success)
                return StatusCode((int)HttpStatusCode.InternalServerError, result);

            return Ok(result);
        }

        [HttpGet]
        [ProducesResponseType<PaginationResponse<RefundResponse>>((int)HttpStatusCode.OK)]
        [ProducesResponseType<ErrorResponse>((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetAllRefunds(
            [FromQuery] RefundRequest refundRequest)
        {
            var result = await _refundService.GetAllRefunds(refundRequest);
            return Ok(result);
        }
    }
}