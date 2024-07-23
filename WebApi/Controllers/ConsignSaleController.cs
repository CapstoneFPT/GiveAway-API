using BusinessObjects.Dtos.Commons;
using BusinessObjects.Dtos.ConsignSaleDetails;
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
            return await _consignsaleService.GetConsignSaleById(consignsaleId);
        }
        [HttpPut("{consignsaleId}/approval")]
        public async Task<ActionResult<Result<ConsignSaleResponse>>> ApprovalConsignSale([FromRoute] Guid consignsaleId, ConsignSaleStatus consignStatus)
        {
            return await _consignsaleService.ApprovalConsignSale(consignsaleId, consignStatus);
        }
        [HttpPut("{consignsaleId}/confirm-received")]
        public async Task<ActionResult<Result<ConsignSaleResponse>>> ConfirmReceivedConsignFromShop([FromRoute] Guid consignsaleId)
        {
            return await _consignsaleService.ConfirmReceivedFromShop(consignsaleId);
        }

        [HttpGet("{consignsaleId}/consignsaledetails")]
        public async Task<ActionResult<Result<List<ConsignSaleDetailResponse>>>> GetConsignSaleDetailsByConsignSaleId(
            [FromRoute] Guid consignsaleId)
        {
            return await _consignsaleService.GetConsignSaleDetailsByConsignSaleId(consignsaleId);
        }
    }
}
