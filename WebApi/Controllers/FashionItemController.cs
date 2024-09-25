using System.Net;
using BusinessObjects.Dtos.AuctionDeposits;
using BusinessObjects.Dtos.AuctionItems;
using BusinessObjects.Dtos.Commons;
using BusinessObjects.Dtos.FashionItems;
using BusinessObjects.Dtos.Orders;
using BusinessObjects.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Services.FashionItems;

namespace WebApi.Controllers
{
    [Route("api/fashionitems")]
    [ApiController]
    public class FashionItemController : ControllerBase
    {
        private readonly IFashionItemService _fashionItemService;

        public FashionItemController(IFashionItemService fashionItemService)
        {
            _fashionItemService = fashionItemService;
        }

        [HttpGet]
        [ProducesResponseType<PaginationResponse<FashionItemList>>((int)HttpStatusCode.OK)]
        [ProducesResponseType<ErrorResponse>((int)HttpStatusCode.InternalServerError)]
        public async Task<ActionResult<Result<PaginationResponse<FashionItemDetailResponse>>>>
            GetAllFashionItemsPagination([FromQuery] FashionItemListRequest request)
        {
            var result = await _fashionItemService.GetAllFashionItemPagination(request);


            return Ok(result);
        }

        [HttpGet("{itemId}")]
        public async Task<ActionResult<Result<FashionItemDetailResponse>>> GetFashionItemById([FromRoute] Guid itemId,
            [FromQuery] Guid? memberId)
        {
            var result = await _fashionItemService.GetFashionItemById(itemId, memberId);

            if (result.ResultStatus != ResultStatus.Success)
                return StatusCode((int)HttpStatusCode.InternalServerError, result);

            return Ok(result);
        }

        [HttpPatch("{itemId}/add-returned-item")]
        [ProducesResponseType<FashionItemDetailResponse>((int)HttpStatusCode.OK)]
        [ProducesResponseType<ErrorResponse>((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> AddReturnedItem([FromRoute] Guid itemId)
        {
            var result = await _fashionItemService.AddReturnedItemToShop(itemId);

            if (!result.IsSuccessful)
                return result.Error switch
                {
                    ErrorCode.InvalidOperation => StatusCode((int)HttpStatusCode.Conflict, new ErrorResponse(
                        "This item is not returned yet", ErrorType.ApiError, HttpStatusCode.Conflict, result.Error)),
                    _ => StatusCode((int)HttpStatusCode.InternalServerError, new ErrorResponse(
                        "Something went wrong", ErrorType.ApiError, HttpStatusCode.InternalServerError, result.Error))
                };

            return Ok(result.Value);
        }

        [Authorize(Roles = "Staff,Admin")]
        [HttpGet("export-excel")]
        [ProducesResponseType<FileContentResult>((int)HttpStatusCode.OK)]
        [ProducesResponseType<ErrorResponse>((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> ExportCsv([FromQuery] ExportFashionItemsRequest request)
        {
            var result = await _fashionItemService.ExportFashionItemsToExcel(request);

            if (!result.IsSuccessful)
                return StatusCode((int)HttpStatusCode.InternalServerError, new ErrorResponse(
                    "Something went wrong", ErrorType.ApiError, HttpStatusCode.InternalServerError, result.Error));

            return File(result.Value.Content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"{result.Value.FileName}");
        }

        [Authorize(Roles = "Staff,Admin")]
        [HttpPut("{itemid}/check-availability")]
        public async Task<ActionResult<Result<FashionItemDetailResponse>>> CheckFashionItemAvailability(
            [FromRoute] Guid itemid)
        {
            var result = await _fashionItemService.CheckFashionItemAvailability(itemid);

            if (result.ResultStatus != ResultStatus.Success)
                return StatusCode((int)HttpStatusCode.InternalServerError, result);

            return Ok(result);
        }
        [Authorize(Roles = "Staff,Admin")]
        [HttpPut("{itemId}")]
        public async Task<ActionResult<Result<FashionItemDetailResponse>>> UpdateFashionItem([FromRoute] Guid itemId,
            [FromBody] UpdateFashionItemRequest request)
        {
            var result = await _fashionItemService.UpdateFashionItem(itemId, request);

            if (result.ResultStatus != ResultStatus.Success)
                return StatusCode((int)HttpStatusCode.InternalServerError, result);

            return Ok(result);
        }
        [Authorize(Roles = "Staff,Admin")]
        [HttpPatch("{itemId}/status")]
        public async Task<ActionResult<Result<FashionItemDetailResponse>>> UpdateFashionItemStatus(
            [FromRoute] Guid itemId, [FromBody] UpdateFashionItemStatusRequest request)
        {
            var result = await _fashionItemService.UpdateFashionItemStatus(itemId, request);

            if (result.ResultStatus != ResultStatus.Success)
                return StatusCode((int)HttpStatusCode.InternalServerError, result);

            return Ok(result);
        }
        [Authorize(Roles = "Admin")]
        [HttpDelete]
        public async Task<IActionResult> DeleteDraftItem(
            [FromBody] List<DeleteDraftItemRequest> deleteDraftItemRequests)
        {
            var result = await _fashionItemService.DeleteDraftItem(deleteDraftItemRequests);

            if (result.ResultStatus != ResultStatus.Success)
                return StatusCode((int)HttpStatusCode.InternalServerError, result);

            return Ok(result);
        }
    }


}