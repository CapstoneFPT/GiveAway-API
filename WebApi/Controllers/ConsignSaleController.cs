using System.Net;
using BusinessObjects.Dtos.Commons;
using BusinessObjects.Dtos.ConsignSaleLineItems;
using BusinessObjects.Dtos.ConsignSales;
using BusinessObjects.Dtos.FashionItems;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Services.ConsignSales;

namespace WebApi.Controllers
{
    [Route("api/consignsales")]
    [ApiController]
    public class ConsignSaleController : ControllerBase
    {
        private readonly IConsignSaleService _consignSaleService;

        public ConsignSaleController(IConsignSaleService consignSaleService)
        {
            _consignSaleService = consignSaleService;
        }
        [HttpGet]
        [ProducesResponseType<PaginationResponse<ConsignSaleListResponse>>((int)HttpStatusCode.OK)]
        [ProducesResponseType<ErrorResponse>((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetConsignSales(
            [FromQuery] ConsignSaleListRequest request)
        {
            var result = await _consignSaleService.GetConsignSales(request);

            if (!result.IsSuccessful)
            {
                return result.Error switch
                {
                    _ => StatusCode(500, new ErrorResponse("Error fetching consign sales", ErrorType.ApiError,
                        HttpStatusCode.InternalServerError, ErrorCode.UnknownError))
                };
            }

            return Ok(result.Value);
        }
        
        [HttpGet("{consignSaleId}")]
        [ProducesResponseType<ConsignSaleDetailedResponse>((int) HttpStatusCode.OK)]
        [ProducesResponseType<ErrorResponse>((int) HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetConsignSaleById([FromRoute] Guid consignSaleId)
        {
            var result = await _consignSaleService.GetConsignSaleById(consignSaleId);

            if (!result.IsSuccessful)
            {
                return result.Error switch
                {
                    _ => StatusCode(500, new ErrorResponse("Error fetching consign sale details", ErrorType.ApiError,
                        HttpStatusCode.InternalServerError, result.Error))
                };
            } 

            return Ok(result.Value);
        }

        [HttpPut("{consignSaleId}/approval")]
        public async Task<ActionResult<Result<ConsignSaleDetailedResponse>>> ApprovalConsignSale([FromRoute] Guid consignSaleId,
            ApproveConsignSaleRequest request)
        {
            var result = await _consignSaleService.ApprovalConsignSale(consignSaleId, request);

            if (result.ResultStatus != ResultStatus.Success)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, result);
            }

            return Ok(result);
        }



        [HttpPut("{consignSaleId}/confirm-received")]
        [ProducesResponseType<Result<MasterItemResponse>>((int)HttpStatusCode.OK)]
        [ProducesResponseType<ErrorResponse>((int)HttpStatusCode.InternalServerError)]
        public async Task<ActionResult<Result<ConsignSaleDetailedResponse>>> ConfirmReceivedConsignFromShop(
            [FromRoute] Guid consignSaleId)
        {
            var result = await _consignSaleService.ConfirmReceivedFromShop(consignSaleId);

            if (result.ResultStatus != ResultStatus.Success)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, result);
            }

            return Ok(result);
        }

        [HttpPost("{consignsaleId}/create-masteritem")]
        [ProducesResponseType<Result<MasterItemResponse>>((int)HttpStatusCode.OK)]
        [ProducesResponseType<ErrorResponse>((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> CreateMasterItemFromConsignSaleLineItem([FromRoute] Guid consignsaleId,
            [FromBody] CreateMasterItemForConsignRequest detailRequest)
        {
            var result = await _consignSaleService.CreateMasterItemFromConsignSaleLineItem(consignsaleId, detailRequest);
            if (result.ResultStatus != ResultStatus.Success)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, result);
            }

            return Ok(result);
        }

        [HttpPost("fashionitems/{masteritemId}/create-variation")]
        [ProducesResponseType<Result<ItemVariationListResponse>>((int)HttpStatusCode.OK)]
        [ProducesResponseType<ErrorResponse>((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> CreateVariationFromConsignSaleLineItem([FromRoute] Guid masteritemId,
            [FromBody] CreateItemVariationRequestForConsign request)
        {
            var result = await _consignSaleService.CreateVariationFromConsignSaleLineItem(masteritemId, request);
            return result.ResultStatus != ResultStatus.Success
                ? StatusCode((int)HttpStatusCode.InternalServerError, result)
                : Ok(result);
        }

      
        [HttpGet("{consignsaleId}/consignlineitems")]
        [ProducesResponseType<List<ConsignSaleLineItemsListResponse>>((int)HttpStatusCode.OK)]
        [ProducesResponseType<ErrorResponse>((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetConsignSaleLineItemsByConsignSaleId(
            [FromRoute] Guid consignsaleId)
        {
            var result = await _consignSaleService.GetConsignSaleLineItems(consignsaleId);

            if (!result.IsSuccessful)

            {
                return result.Error switch
                {
                    ErrorCode.ServerError => StatusCode(500,
                        new ErrorResponse("Error fetching consign sale details", ErrorType.ApiError,
                            HttpStatusCode.InternalServerError, ErrorCode.ServerError)),
                    _ => StatusCode(500,
                        new ErrorResponse("Error fetching consign sale details", ErrorType.ApiError,
                            HttpStatusCode.InternalServerError, ErrorCode.UnknownError))
                };
            }

            return Ok(result.Value);
        }
    }
}