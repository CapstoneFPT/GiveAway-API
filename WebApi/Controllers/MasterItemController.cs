using System.Net;
using BusinessObjects.Dtos.Commons;
using BusinessObjects.Dtos.FashionItems;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services.FashionItems;

namespace WebApi.Controllers;
[Route("api/master-items")]
[ApiController]
public class MasterItemController : ControllerBase
{
    private readonly IFashionItemService _fashionItemService;

    public MasterItemController(IFashionItemService fashionItemService)
    {
        _fashionItemService = fashionItemService;
    }

    [HttpGet]
    [ProducesResponseType<PaginationResponse<MasterItemListResponse>>((int)HttpStatusCode.OK)]
    [ProducesResponseType<ErrorResponse>((int)HttpStatusCode.InternalServerError)]
    public async Task<IActionResult> GetAllMasterItemPagination([FromQuery] MasterItemRequest request)
    {
        var result = await _fashionItemService.GetAllMasterItemPagination(request);

        return Ok(result);
    }
    [HttpGet("frontpage")]
    [ProducesResponseType<PaginationResponse<MasterItemListResponse>>((int)HttpStatusCode.OK)]
    [ProducesResponseType<ErrorResponse>((int)HttpStatusCode.InternalServerError)]
    public async Task<IActionResult> GetAllMasterItemPaginationFrontpage([FromQuery] FrontPageMasterItemRequest request)
    {
        var result = await _fashionItemService.GetMasterItemFrontPage(request);
        return Ok(result);
    }

    [HttpGet("find")]
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
    [HttpGet("{masterItemId}")]
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
    [Authorize(Roles = "Admin")]
    [HttpPost]
    [ProducesResponseType<Result<MasterItemResponse>>((int)HttpStatusCode.OK)]
    public async Task<IActionResult> CreateMasterItem([FromBody] CreateMasterItemRequest request)
    {
        var result = await _fashionItemService.CreateMasterItemByAdmin(request);
        if (result.ResultStatus != ResultStatus.Success)
            return StatusCode((int)HttpStatusCode.InternalServerError, result);
        return Ok(result);
    }
    [Authorize(Roles = "Admin")]
    [HttpPut("{masteritemId}/update-masteritem")]
    [ProducesResponseType<Result<MasterItemResponse>>((int)HttpStatusCode.OK)]
    public async Task<IActionResult> UpdateMasterItem([FromRoute] Guid masteritemId,
        [FromBody] UpdateMasterItemRequest request)
    {
        var result = await _fashionItemService.UpdateMasterItem(masteritemId, request);
        if (result.ResultStatus != ResultStatus.Success)
            return StatusCode((int)HttpStatusCode.InternalServerError, result);
        return Ok(result);
    }
    [Authorize(Roles = "Admin")]
    [HttpPost("{masterItemId}/individual-items")]
    [ProducesResponseType<Result<List<IndividualItemListResponse>>>((int)HttpStatusCode.OK)]
    [ProducesResponseType<ErrorResponse>((int)HttpStatusCode.InternalServerError)]
    public async Task<IActionResult> CreateIndividualItems([FromRoute] Guid masterItemId,
        CreateIndividualItemRequest request)
    {
        var result = await _fashionItemService.CreateIndividualItems(masterItemId, request);
        if (result.ResultStatus != ResultStatus.Success)
            return StatusCode((int)HttpStatusCode.InternalServerError, result);
        return Ok(result);
    }
}