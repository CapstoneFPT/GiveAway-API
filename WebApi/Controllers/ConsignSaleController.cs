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
        [HttpGet("consignsales/{consignsaleId}")]
        public async Task<ActionResult<Result<ConsignSaleResponse>>> GetConsignSaleById([FromRoute] Guid accountId, [FromRoute] Guid consignsaleId)
        {
            return await _consignsaleService.GetConsignSaleById(accountId, consignsaleId);
        }
    }
}
