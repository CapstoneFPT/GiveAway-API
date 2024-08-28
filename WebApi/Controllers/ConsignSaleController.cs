using System.Net;
using BusinessObjects.Dtos.Commons;
using BusinessObjects.Dtos.ConsignSaleDetails;
using BusinessObjects.Dtos.ConsignSales;
using BusinessObjects.Dtos.FashionItems;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Services.ConsignSales;

namespace WebApi.Controllers
{
    [Route("api/consginsales")]
    [ApiController]
    public class ConsignSaleController : ControllerBase
    {
        private readonly IConsignSaleService _consignsaleService;

        public ConsignSaleController(IConsignSaleService consignsaleService)
        {
            _consignsaleService = consignsaleService;
        }
        [HttpGet]
        [ProducesResponseType<PaginationResponse<ConsignSaleListResponse>>((int)HttpStatusCode.OK)]
        [ProducesResponseType<ErrorResponse>((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetConsignSales(
            [FromQuery] ConsignSaleListRequest request)
        {
            var result = await _consignsaleService.GetConsignSales(request);

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
        
        [HttpGet("{consignsaleId}")]
        public async Task<ActionResult<Result<ConsignSaleResponse>>> GetConsignSaleById([FromRoute] Guid consignsaleId)
        {
            var result = await _consignsaleService.GetConsignSaleById(consignsaleId);

            if (result.ResultStatus != ResultStatus.Success)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, result);
            }

            return Ok(result);
        }

        [HttpPut("{consignsaleId}/approval")]
        public async Task<ActionResult<Result<ConsignSaleResponse>>> ApprovalConsignSale([FromRoute] Guid consignsaleId,
            ApproveConsignSaleRequest request)
        {
            var result = await _consignsaleService.ApprovalConsignSale(consignsaleId, request);

            if (result.ResultStatus != ResultStatus.Success)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, result);
            }

            return Ok(result);
        }



        [HttpPut("{consignSaleId}/confirm-received")]
        [ProducesResponseType<Result<MasterItemResponse>>((int)HttpStatusCode.OK)]
        [ProducesResponseType<ErrorResponse>((int)HttpStatusCode.InternalServerError)]
        public async Task<ActionResult<Result<ConsignSaleResponse>>> ConfirmReceivedConsignFromShop(
            [FromRoute] Guid consignSaleId)
        {
            var result = await _consignsaleService.ConfirmReceivedFromShop(consignSaleId);

            if (result.ResultStatus != ResultStatus.Success)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, result);
            }

            return Ok(result);
        }

        [HttpPost("{consignsaleId}/create-masteritem")]
        [ProducesResponseType<Result<MasterItemResponse>>((int)HttpStatusCode.OK)]
        [ProducesResponseType<ErrorResponse>((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> CreateMasterItemFromConsignSaleDetail([FromRoute] Guid consignsaleId,
            [FromBody] CreateMasterItemForConsignRequest detailRequest)
        {
            var result = await _consignsaleService.CreateMasterItemFromConsignSaleDetail(consignsaleId, detailRequest);
            if (result.ResultStatus != ResultStatus.Success)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, result);
            }

            return Ok(result);
        }

        [HttpPost("fashionitems/{masteritemId}/create-variation")]
        [ProducesResponseType<Result<ItemVariationListResponse>>((int)HttpStatusCode.OK)]
        [ProducesResponseType<ErrorResponse>((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> CreateVariationFromConsignSaleDetail([FromRoute] Guid masteritemId,
            [FromBody] CreateItemVariationRequestForConsign request)
        {
            var result = await _consignsaleService.CreateVariationFromConsignSaleDetail(masteritemId, request);
            return result.ResultStatus != ResultStatus.Success
                ? StatusCode((int)HttpStatusCode.InternalServerError, result)
                : Ok(result);
        }

        [HttpPost("consignsaledetails/{consignsaledetailId}/fashionitems/{variationId}/create-individual")]
        [ProducesResponseType<Result<FashionItemDetailResponse>>((int)HttpStatusCode.OK)]
        [ProducesResponseType<ErrorResponse>((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> CreateIndividualItemFromConsignSaleDetail([FromRoute] Guid consignsaledetailId,
            [FromRoute] Guid variationId, [FromBody] CreateIndividualItemRequestForConsign request)
        {
            var result = await _consignsaleService.CreateIndividualItemFromConsignSaleDetail(consignsaledetailId,variationId,request);

            return result.ResultStatus != ResultStatus.Success
                ? StatusCode((int)HttpStatusCode.InternalServerError, result)
                : Ok(result);
        }
        [HttpGet("{consignsaleId}/consignsaledetails")]
        [ProducesResponseType<List<ConsignSaleDetailResponse>>((int)HttpStatusCode.OK)]
        [ProducesResponseType<ErrorResponse>((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetConsignSaleDetailsByConsignSaleId(
            [FromRoute] Guid consignsaleId)
        {
            var result = await _consignsaleService.GetConsignSaleDetails(consignsaleId);

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