using System.Net;
using BusinessObjects.Dtos.Commons;
using BusinessObjects.Dtos.ConsignSaleLineItems;
using BusinessObjects.Dtos.FashionItems;
using DotNext;
using Microsoft.AspNetCore.Mvc;
using Services.ConsignLineItems;
using Services.ConsignSales;

namespace WebApi.Controllers;

[ApiController]
[Route("api/consignlineitems")]
public class ConsignLineItemController : ControllerBase
{
    private readonly IConsignSaleService _consignSaleService;
    private readonly IConsignLineItemService _consignLineItemService;

    public ConsignLineItemController(IConsignSaleService consignSaleService, IConsignLineItemService consignLineItemService)
    {
        _consignSaleService = consignSaleService;
        _consignLineItemService = consignLineItemService;
    }

    [HttpPost("{consignLineItemId}/fashionitems/{masterItemId}/create-individual")]
    [ProducesResponseType<BusinessObjects.Dtos.Commons.Result<FashionItemDetailResponse>>((int)HttpStatusCode.OK)]
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

    [HttpGet("{consignLineItemId}")]
    [ProducesResponseType<ConsignSaleLineItemDetailedResponse>((int)HttpStatusCode.OK)]
    [ProducesResponseType<ErrorResponse>((int)HttpStatusCode.InternalServerError)]
    public async Task<IActionResult> GetDetailedConsignLineItem([FromRoute] Guid consignLineItemId)
    {
        var result = await _consignLineItemService.GetConsignLineItemById(consignLineItemId);

        if (!result.IsSuccessful)
        {
            return result.Error switch
            {
                ErrorCode.NotFound => NotFound(new ErrorResponse("Consign line item not found", ErrorType.ApiError,
                    HttpStatusCode.NotFound, result.Error)),
                _ => StatusCode((int)HttpStatusCode.InternalServerError,
                    new ErrorResponse("Error fetching consign line item details", ErrorType.ApiError,
                        HttpStatusCode.InternalServerError, result.Error))
            };
        }

        return Ok(result.Value);
    }

    [HttpPut("{consignLineItemId}/confirm-price")]
    [ProducesResponseType<BusinessObjects.Dtos.Commons.Result<ConsignSaleLineItemsListResponse>>((int)HttpStatusCode.OK)]
    [ProducesResponseType<ErrorResponse>((int)HttpStatusCode.InternalServerError)]
    public async Task<IActionResult> ConfirmConsignSaleLineItemPrice(Guid consignLineItemId ,decimal price)
    {
        var result =
            await _consignSaleService.ConfirmConsignSaleLineItemPrice(consignLineItemId, price);

        return result.ResultStatus != ResultStatus.Success
            ? StatusCode((int)HttpStatusCode.InternalServerError, result)
            : Ok(result);
    }
}