using System.Net;
using BusinessObjects.Dtos.AuctionDeposits;
using BusinessObjects.Dtos.AuctionItems;
using BusinessObjects.Dtos.Commons;
using BusinessObjects.Dtos.FashionItems;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Services.FashionItems;

namespace WebApi.Controllers
{
    [Route("api/fashionitems")]
    [ApiController]
    [EnableCors("AllowAll")]
    public class FashionItemController : ControllerBase
    {
        private readonly IFashionItemService _fashionItemService;

        public FashionItemController(IFashionItemService fashionItemService)
        {
            _fashionItemService = fashionItemService;
        }

        [HttpGet]
        public async Task<ActionResult<Result<PaginationResponse<FashionItemDetailResponse>>>>
            GetAllFashionItemsPagination([FromQuery] AuctionFashionItemRequest request)
        {
            var result = await _fashionItemService.GetAllFashionItemPagination(request);

            if (result.ResultStatus != ResultStatus.Success)
                return StatusCode((int)HttpStatusCode.InternalServerError, result);

            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Result<FashionItemDetailResponse>>> GetFashionItemById([FromRoute] Guid id)
        {
            var result = await _fashionItemService.GetFashionItemById(id);

            if (result.ResultStatus != ResultStatus.Success)
                return StatusCode((int)HttpStatusCode.InternalServerError, result);

            return Ok(result);
        }

        [HttpPut("{itemid}/check-availability")]
        public async Task<ActionResult<Result<FashionItemDetailResponse>>> CheckFashionItemAvailability(
            [FromRoute] Guid itemid)
        {
            var result = await _fashionItemService.CheckFashionItemAvailability(itemid);

            if (result.ResultStatus != ResultStatus.Success)
                return StatusCode((int)HttpStatusCode.InternalServerError, result);

            return Ok(result);
        }

        [HttpPut("{itemId}")]
        public async Task<ActionResult<Result<FashionItemDetailResponse>>> UpdateFashionItem([FromRoute] Guid itemId,
            [FromBody] UpdateFashionItemRequest request)
        {
            var result = await _fashionItemService.UpdateFashionItem(itemId, request);

            if (result.ResultStatus != ResultStatus.Success)
                return StatusCode((int)HttpStatusCode.InternalServerError, result);

            return Ok(result);
        }

        [HttpPatch("{itemId}/status")]
        public async Task<ActionResult<Result<FashionItemDetailResponse>>> UpdateFashionItemStatus(
            [FromRoute] Guid itemId, [FromBody] UpdateFashionItemStatusRequest request)
        {
            var result = await _fashionItemService.UpdateFashionItemStatus(itemId, request);

            if (result.ResultStatus != ResultStatus.Success)
                return StatusCode((int)HttpStatusCode.InternalServerError, result);

            return Ok(result);
        }
    }
}