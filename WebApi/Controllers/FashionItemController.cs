using System.Net;
using BusinessObjects.Dtos.AuctionDeposits;
using BusinessObjects.Dtos.AuctionItems;
using BusinessObjects.Dtos.Commons;
using BusinessObjects.Dtos.FashionItems;
using BusinessObjects.Entities;
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

        [HttpGet("master-items")]
        [ProducesResponseType<PaginationResponse<MasterItemListResponse>>((int)HttpStatusCode.OK)]
        [ProducesResponseType<ErrorResponse>((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetAllMasterItemPagination([FromQuery] MasterItemRequest request)
        {
            var result = await _fashionItemService.GetAllMasterItemPagination(request);

            return Ok(result);
        }

        [HttpGet("master-items/frontpage")]
        [ProducesResponseType<PaginationResponse<MasterItemListResponse>>((int)HttpStatusCode.OK)]
        [ProducesResponseType<ErrorResponse>((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetAllMasterItemPaginationFrontpage([FromQuery] FrontPageMasterItemRequest request)
        {
            var result = await _fashionItemService.GetMasterItemFrontPage(request);
            return Ok(result);
        }

        [HttpGet("master-items/find")]
        [ProducesResponseType<MasterItemDetailResponse>((int)HttpStatusCode.OK)]
        [ProducesResponseType<ErrorResponse>((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> FindMasterItem([FromQuery] FindMasterItemRequest request)
        {
          var result = await _fashionItemService.FindMasterItem(request);

          if (!result.IsSuccessful)
          {
              return result.Error
                  switch
                  {
                      ErrorCode.NotFound => NotFound(new ErrorResponse("Master Item Not Found",ErrorType.FashionItemError,HttpStatusCode.NotFound,ErrorCode.NotFound)),
                      _ => StatusCode((int)HttpStatusCode.InternalServerError, 
                          new ErrorResponse("Unknown Error",ErrorType.ApiError,HttpStatusCode.InternalServerError,ErrorCode.UnknownError))
                  };
          }

          return Ok(result.Value);
        }
            
        
        /*[HttpGet("{masteritemId}/item-variants")]
        [ProducesResponseType<PaginationResponse<ItemVariationListResponse>>((int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetAllItemVariationPagination([FromRoute] Guid masteritemId,
            [FromQuery] ItemVariationRequest request)
        {
            var result = await _fashionItemService.GetAllFashionItemVariationPagination(masteritemId, request);


            return Ok(result);
        }*/

        [HttpGet("master-items/{masterItemId}")]
        [ProducesResponseType<MasterItemDetailResponse>((int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetMasterItemDetail([FromRoute] Guid masterItemId)
        {
            var result = await _fashionItemService.GetMasterItemById(masterItemId);

            if (!result.IsSuccessful)
            {
                return result.Error switch
                {
                    ErrorCode.NotFound => NotFound(result),
                    _ => StatusCode((int)HttpStatusCode.InternalServerError, result)
                };
            }

            return Ok(result.Value);
        }
        

        [HttpGet("{masterItemId}/individual-items")]
        [ProducesResponseType<PaginationResponse<IndividualItemListResponse>>((int)HttpStatusCode.OK)]
        [ProducesResponseType<ErrorResponse>((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetAllIndividualItemPagination([FromRoute] Guid masterItemId,
            [FromQuery] IndividualItemRequest request)
        {
            var result = await _fashionItemService.GetIndividualItemPagination(masterItemId, request);

            return Ok(result);
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
        public async Task<ActionResult<Result<FashionItemDetailResponse>>> GetFashionItemById([FromRoute] Guid itemId, [FromQuery] Guid? memberId)
        {
            var result = await _fashionItemService.GetFashionItemById(itemId,memberId);

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

        [HttpPost("master-items")]
        [ProducesResponseType<Result<MasterItemResponse>>((int)HttpStatusCode.OK)]
        public async Task<IActionResult> CreateMasterItem([FromBody] CreateMasterItemRequest request)
        {
            var result = await _fashionItemService.CreateMasterItemByAdmin(request);
            if (result.ResultStatus != ResultStatus.Success)
                return StatusCode((int)HttpStatusCode.InternalServerError, result);
            return Ok(result);
        }

        /*[HttpPost("{masteritemId}/item-variants")]
        [ProducesResponseType<Result<ItemVariationResponse>>((int)HttpStatusCode.OK)]
        public async Task<IActionResult> CreateItemVariation([FromRoute] Guid masteritemId,
            [FromBody] CreateItemVariationRequest request)
        {
            var result = await _fashionItemService.CreateItemVariation(masteritemId, request);
            if (result.ResultStatus != ResultStatus.Success)
                return StatusCode((int)HttpStatusCode.InternalServerError, result);
            return Ok(result);
        }*/

        [HttpPost("{masterItemId}/individual-items")]
        [ProducesResponseType<Result<List<IndividualItemListResponse>>>((int)HttpStatusCode.OK)]
        [ProducesResponseType<ErrorResponse>((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> CreateIndividualItems([FromRoute] Guid masterItemId,
            List<CreateIndividualItemRequest> request)
        {
            var result = await _fashionItemService.CreateIndividualItems(masterItemId, request);
            if (result.ResultStatus != ResultStatus.Success)
                return StatusCode((int)HttpStatusCode.InternalServerError, result);
            return Ok(result);
        }
    }
}