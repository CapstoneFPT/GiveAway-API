using System.Net;
using BusinessObjects.Dtos.Commons;
using BusinessObjects.Dtos.Refunds;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NuGet.Protocol;
using Services.OrderLineItems;
using Services.Refunds;

namespace WebApi.Controllers
{
    [Authorize]
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

            if (!result.IsSuccessful)
            {
                return result.Error switch
                {
                    ErrorCode.NotFound => StatusCode((int)HttpStatusCode.InternalServerError,
                        new ErrorResponse("Refund not found", ErrorType.ApiError, HttpStatusCode.NotFound,
                            result.Error)),
                    _ => StatusCode((int)HttpStatusCode.InternalServerError,
                        new ErrorResponse("Internal server error", ErrorType.ApiError,
                            HttpStatusCode.InternalServerError, result.Error))
                };
            }

            return Ok(result.Value);
        }
        [Authorize(Roles = "Staff")]
        [HttpPut("{refundId}/approval")]
        public async Task<ActionResult<Result<RefundResponse>>> ApprovalRefundRequestFromShop([FromRoute] Guid refundId,
            [FromBody] ApprovalRefundRequest request)
        {
            var result = await _refundService.ApprovalRefundRequestFromShop(refundId, request);

            if (result.ResultStatus != ResultStatus.Success)
                return StatusCode((int)HttpStatusCode.InternalServerError, result);

            return Ok(result);
        }
        [Authorize(Roles = "Staff")]
        [HttpPut("{refundId}/confirm-received-and-refund")]
        public async Task<ActionResult<Result<RefundResponse>>> ConfirmReceivedAndRefund([FromRoute] Guid refundId, [FromBody] ConfirmReceivedRequest request)
        {
            var result = await _refundService.ConfirmReceivedAndRefund(refundId, request);
            if (result.ResultStatus != ResultStatus.Success)
                return StatusCode((int)HttpStatusCode.InternalServerError, result);
            return Ok(result);
        }
        [Authorize(Roles = "Member")]
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

        [HttpPut("{refundId}/cancel")]
        [ProducesResponseType<RefundResponse>((int)HttpStatusCode.OK)]
        [ProducesResponseType<ErrorResponse>((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> CancelRefund([FromRoute] Guid refundId)
        {
            var result = await _refundService.CancelRefund(refundId);
            if (!result.IsSuccessful)
            {
                return result.Error switch
                {
                    ErrorCode.NotFound => NotFound(new ErrorResponse("Refund not found", ErrorType.RefundError,
                        HttpStatusCode.NotFound, result.Error)),
                    ErrorCode.RefundStatusNotAvailable => StatusCode((int)HttpStatusCode.Conflict,
                        new ErrorResponse("Refund status not available", ErrorType.RefundError, HttpStatusCode.Conflict,
                            result.Error)),
                    _ => StatusCode((int)HttpStatusCode.InternalServerError,
                        new ErrorResponse("Internal server error", ErrorType.RefundError,
                            HttpStatusCode.InternalServerError, result.Error))
                };
            }

            return Ok(result.Value);
        }
        [HttpPut("{refundId}/update")]
        [ProducesResponseType<RefundResponse>((int)HttpStatusCode.OK)]
        [ProducesResponseType<ErrorResponse>((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> UpdateRefund([FromRoute] Guid refundId, [FromBody] UpdateRefundRequest request)
        {
            var result = await _refundService.UpdateRefund(refundId, request);
            if (!result.IsSuccessful)
            {
                return result.Error switch
                {
                    ErrorCode.NotFound => NotFound(new ErrorResponse("Refund not found", ErrorType.RefundError,
                        HttpStatusCode.NotFound, result.Error)),
                    ErrorCode.RefundStatusNotAvailable => StatusCode((int)HttpStatusCode.Conflict,
                        new ErrorResponse("Refund status not available", ErrorType.RefundError, HttpStatusCode.Conflict,
                            result.Error)),
                    ErrorCode.MissingFeature => StatusCode((int)HttpStatusCode.NotAcceptable,
                        new ErrorResponse("Missing valid feature", ErrorType.RefundError, HttpStatusCode.NotAcceptable,
                            result.Error)),
                    _ => StatusCode((int)HttpStatusCode.InternalServerError,
                        new ErrorResponse("Internal server error", ErrorType.RefundError,
                            HttpStatusCode.InternalServerError, result.Error))
                };
            }

            return Ok(result.Value);
        }
    }
}