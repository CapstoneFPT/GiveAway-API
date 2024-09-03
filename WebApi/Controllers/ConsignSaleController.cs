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
        [ProducesResponseType<Result<ConsignSaleDetailedResponse>>((int)HttpStatusCode.OK)]
        [ProducesResponseType<ErrorResponse>((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> ApprovalConsignSale([FromRoute] Guid consignSaleId,
            ApproveConsignSaleRequest request)
        {
            var result = await _consignSaleService.ApprovalConsignSale(consignSaleId, request);

            if (result.ResultStatus != ResultStatus.Success)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, result);
            }

            return Ok(result);
        }
        [HttpPut("{consignSaleId}/post-items-to-sell")]
        [ProducesResponseType<Result<ConsignSaleDetailedResponse>>((int)HttpStatusCode.OK)]
        [ProducesResponseType<ErrorResponse>((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> PostConsignSaleForSelling([FromRoute] Guid consignSaleId)
        {
            var result = await _consignSaleService.PostConsignSaleForSelling(consignSaleId);

            if (result.ResultStatus != ResultStatus.Success)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, result);
            }

            return Ok(result);
        }
        [HttpPut("{consignSaleId}/negotiating")]
        [ProducesResponseType<Result<ConsignSaleDetailedResponse>>((int)HttpStatusCode.OK)]
        [ProducesResponseType<ErrorResponse>((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> NegotiatingConsignSale([FromRoute] Guid consignSaleId)
        {
            var result = await _consignSaleService.NegotiatingConsignSale(consignSaleId);

            if (result.ResultStatus != ResultStatus.Success)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, result);
            }

            return Ok(result);
        }
        [HttpPut("{consignSaleId}/ready-to-sale")]
        [ProducesResponseType<Result<ConsignSaleDetailedResponse>>((int)HttpStatusCode.OK)]
        [ProducesResponseType<ErrorResponse>((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> ReadyToSaleConsignSale([FromRoute] Guid consignSaleId)
        {
            var result = await _consignSaleService.ReadyToSaleConsignSale(consignSaleId);

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

        [HttpPut("{consignsaleId}/notify-delivery")]
        public async Task<IActionResult> NotifyDelivery([FromRoute] Guid consignsaleId)
        {
            DotNext.Result<ConsignSaleDetailedResponse, ErrorCode> result =
                await _consignSaleService.NotifyDelivery(consignsaleId);

            if (!result.IsSuccessful)
            {
                return result.Error switch
                {
                    _ => StatusCode(500,
                        new ErrorResponse("Error updating consign sale details", ErrorType.ApiError,
                            HttpStatusCode.InternalServerError, result.Error))
                };
            }

            return Ok(result.Value);
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

        [HttpPut("{consignsaleId}/cancel-all-consignsaleline")]
        [ProducesResponseType<Result<ConsignSaleDetailedResponse>>((int)HttpStatusCode.OK)]
        [ProducesResponseType<ErrorResponse>((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> CancelAllConsignSaleLineItems([FromRoute] Guid consignsaleId)
        {
            var result = await _consignSaleService.CancelAllConsignSaleLineItems(consignsaleId);

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