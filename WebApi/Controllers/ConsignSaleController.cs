using System.Net;
using BusinessObjects.Dtos.Commons;
using BusinessObjects.Dtos.ConsignSales;
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
            ConsignSaleStatus consignStatus)
        {
            var result = await _consignsaleService.ApprovalConsignSale(consignsaleId, consignStatus);
            
            if(result.ResultStatus != ResultStatus.Success)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, result);
            }
            
            return Ok(result);
        }

        [HttpPut("{consignsaleId}/confirm-received")]
        public async Task<ActionResult<Result<ConsignSaleResponse>>> ConfirmReceivedConsignFromShop(
            [FromRoute] Guid consignsaleId)
        {
            var result = await _consignsaleService.ConfirmReceivedFromShop(consignsaleId);
            
            if(result.ResultStatus != ResultStatus.Success)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, result);
            }
            
            return Ok(result);
        }
    }
}