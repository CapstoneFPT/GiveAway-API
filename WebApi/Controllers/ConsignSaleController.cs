using System.Net;
using BusinessObjects.Dtos.Commons;
using BusinessObjects.Dtos.ConsignSaleLineItems;
using BusinessObjects.Dtos.ConsignSales;
using BusinessObjects.Dtos.FashionItems;
using Microsoft.AspNetCore.Authorization;
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
        [Authorize(Roles = "Staff,Admin")]
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
        [Authorize]
        [HttpGet("{consignSaleId}")]
        [ProducesResponseType<ConsignSaleDetailedResponse>((int)HttpStatusCode.OK)]
        [ProducesResponseType<ErrorResponse>((int)HttpStatusCode.InternalServerError)]
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
        [Authorize(Roles = "Staff")]
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
        [Authorize(Roles = "Staff,Admin")]
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
        [Authorize(Roles = "Staff")]
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
        /*[HttpPut("{consignSaleId}/ready-to-sale")]
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
        }*/
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
        [Authorize(Roles = "Staff")]
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

        [Authorize(Roles = "Staff,Admin")]
        [HttpGet("export-excel")]
        [ProducesResponseType(typeof(FileContentResult), (int)HttpStatusCode.OK)]
        [ProducesResponseType<ErrorResponse>((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> ExportConsignSaleToExcel([FromQuery] ExportConsignSaleToExcelRequest request)
        {
            var result = await _consignSaleService.ExportConsignSaleToExcel(request);

            if (!result.IsSuccessful)
            {
                return result.Error switch
                {
                    ErrorCode.ServerError => StatusCode(500,
                        new ErrorResponse("Error exporting consign sale to Excel", ErrorType.ApiError,
                            HttpStatusCode.InternalServerError, ErrorCode.ServerError)),
                    _ => StatusCode(500,
                        new ErrorResponse("Error exporting consign sale to Excel", ErrorType.ApiError,
                            HttpStatusCode.InternalServerError, ErrorCode.UnknownError))
                };
            }

            return File(result.Value.Content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"ConsignSale_{DateTime.Now:yyyyMMddHHmmss}.xlsx");
        }

        [Authorize]
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
        [Authorize(Roles = "Member")]
        [HttpPut("{consignsaleId}/continue-consignsale")]
        [ProducesResponseType<Result<ConsignSaleDetailedResponse>>((int)HttpStatusCode.OK)]
        [ProducesResponseType<ErrorResponse>((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> ContinueConsignSale([FromRoute] Guid consignsaleId)
        {
            var result = await _consignSaleService.ContinueConsignSale(consignsaleId);

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
        [Authorize]
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
        [Authorize(Roles = "Staff,Admin")]
        [HttpGet("{consignsaleId}/invoice")]
        [ProducesResponseType(typeof(FileContentResult), (int)HttpStatusCode.OK)]
        [ProducesResponseType<ErrorResponse>((int)HttpStatusCode.NotFound)]
        [ProducesResponseType<ErrorResponse>((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GenerateConsignInvoice([FromRoute] Guid consignsaleId,
            [FromQuery] Guid shopId)
        {
            var result = await _consignSaleService.GenerateConsignOfflineInvoice(consignsaleId, shopId);

            if (!result.IsSuccessful)
            {
                return result.Error switch
                {
                    ErrorCode.NotFound => NotFound(new ErrorResponse("Order not found", ErrorType.ApiError,
                        HttpStatusCode.NotFound, result.Error)),
                    _ => StatusCode(500,
                        new ErrorResponse("Error generating invoice", ErrorType.ApiError,
                            HttpStatusCode.InternalServerError, result.Error))
                };
            }

            return File(result.Value.Content, "application/pdf", $"Invoice_{result.Value.ConsignSaleCode}.pdf");
        }
    }

}