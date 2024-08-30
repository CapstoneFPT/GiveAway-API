using System.Net;
using BusinessObjects.Dtos.Commons;
using BusinessObjects.Dtos.FashionItems;
using Microsoft.AspNetCore.Mvc;
using Services.ConsignSales;

namespace WebApi.Controllers;

[ApiController]
[Route("api/consignlineitems")]
public class ConsignLineItemController : ControllerBase
{
    private readonly IConsignSaleService _consignSaleService;

    public ConsignLineItemController(IConsignSaleService consignSaleService)
    {
        _consignSaleService = consignSaleService;
    }

    [HttpPost("{consignLineItemId}/fashionitems/{masterItemId}/create-individual")]
    [ProducesResponseType<Result<FashionItemDetailResponse>>((int)HttpStatusCode.OK)]
    [ProducesResponseType<ErrorResponse>((int)HttpStatusCode.InternalServerError)]
    public async Task<IActionResult> CreateIndividualItemFromConsignSaleListItem([FromRoute] Guid consignLineItemId,
        [FromRoute] Guid masterItemId, [FromBody] CreateIndividualItemRequestForConsign request)
    {
        var result =
            await _consignSaleService.CreateIndividualItemFromConsignSaleLineItem(consignLineItemId, masterItemId,
                request);

        return result.ResultStatus != ResultStatus.Success
            ? StatusCode((int)HttpStatusCode.InternalServerError, result)
            : Ok(result);
    }
}