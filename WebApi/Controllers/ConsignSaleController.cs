using System.Net;
using BusinessObjects.Dtos.Commons;
using BusinessObjects.Dtos.ConsignSaleDetails;
using BusinessObjects.Dtos.ConsignSales;
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
        public async Task<ActionResult<Result<PaginationResponse<ConsignSaleResponse>>>> GetAllConsignSaleByShopId(
            [FromQuery] ConsignSaleRequestForShop request)
        {
            var result = await _consignsaleService.GetAllConsignSalesByShopId(request);

            if (result.ResultStatus != ResultStatus.Success)
                return StatusCode((int)HttpStatusCode.InternalServerError, result);

            return Ok(result);
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

        [HttpPut("consignsaledetails/{consignsaledetailId}/update-for-approve")]
        public async Task<ActionResult<Result<ConsignSaleDetailResponse>>> UpdateConsignSaleDetailForApprove([FromRoute] Guid consignsaledetailId,
            [FromBody] ConfirmReceivedConsignRequest request)
        {
            var result = await _consignsaleService.UpdateConsignSaleDetailForApprove(consignsaledetailId,request);
            if (result.ResultStatus != ResultStatus.Success)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, result);
            }

            return Ok(result);
        }
        [HttpGet("{consignsaleId}/consignsaledetails")]
        public async Task<ActionResult<Result<List<ConsignSaleDetailResponse>>>> GetConsignSaleDetailsByConsignSaleId(
            [FromRoute] Guid consignsaleId)
        {
            var result = await _consignsaleService.GetConsignSaleDetailsByConsignSaleId(consignsaleId);

            return result.ResultStatus != ResultStatus.Success
                ? StatusCode((int)HttpStatusCode.InternalServerError, result)
                : Ok(result);
        }
    }

}