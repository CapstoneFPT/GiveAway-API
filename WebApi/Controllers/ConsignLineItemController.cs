using System.Net;
using BusinessObjects.Dtos.Commons;
using BusinessObjects.Dtos.ConsignSaleLineItems;
using BusinessObjects.Dtos.ConsignSales;
using BusinessObjects.Dtos.FashionItems;
using DotNext;
using Microsoft.AspNetCore.Authorization;
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
    [Authorize(Roles = "Staff")]
    [HttpPut("{consignLineItemId}/ready-for-consign")]
    [ProducesResponseType<BusinessObjects.Dtos.Commons.Result<ConsignSaleLineItemResponse>>((int)HttpStatusCode.OK)]
    [ProducesResponseType<ErrorResponse>((int)HttpStatusCode.InternalServerError)]
    public async Task<IActionResult> ConfirmConsignSaleLineReadyToSale([FromRoute] Guid consignLineItemId, [FromBody] ConfirmConsignSaleLineReadyToSaleRequest request)
    {
        var result =
            await _consignSaleService.ConfirmConsignSaleLineReadyToSale(consignLineItemId,request);

        return result.ResultStatus != ResultStatus.Success
            ? StatusCode((int)HttpStatusCode.InternalServerError, result)
            : Ok(result);
    }
    [Authorize(Roles = "Staff")]
    [HttpPut("{consignLineItemId}/negotiate-item")]
    [ProducesResponseType<BusinessObjects.Dtos.Commons.Result<ConsignSaleLineItemResponse>>((int)HttpStatusCode.OK)]
    [ProducesResponseType<ErrorResponse>((int)HttpStatusCode.InternalServerError)]
    public async Task<IActionResult> NegotiateFromConsignSaleLineItem([FromRoute] Guid consignLineItemId, [FromBody] NegotiateConsignSaleLineRequest request)
    {
        var result =
            await _consignSaleService.NegotiateConsignSaleLineItem(consignLineItemId,request);

        return result.ResultStatus != ResultStatus.Success
            ? StatusCode((int)HttpStatusCode.InternalServerError, result)
            : Ok(result);
    }
    [Authorize(Roles = "Member")]
    [HttpPut("{consignLineItemId}/aprrove-negotiation")]
    [ProducesResponseType<BusinessObjects.Dtos.Commons.Result<ConsignSaleLineItemResponse>>((int)HttpStatusCode.OK)]
    [ProducesResponseType<ErrorResponse>((int)HttpStatusCode.InternalServerError)]
    public async Task<IActionResult> ApproveNegotiationForConsignSaleLineItem([FromRoute] Guid consignLineItemId)
    {
        var result =
            await _consignSaleService.ApproveNegotiation(consignLineItemId);

        return result.ResultStatus != ResultStatus.Success
            ? StatusCode((int)HttpStatusCode.InternalServerError, result)
            : Ok(result);
    }
    [Authorize(Roles = "Member")]
    [HttpPut("{consignLineItemId}/reject-negotiation")]
    [ProducesResponseType<BusinessObjects.Dtos.Commons.Result<ConsignSaleLineItemResponse>>((int)HttpStatusCode.OK)]
    [ProducesResponseType<ErrorResponse>((int)HttpStatusCode.InternalServerError)]
    public async Task<IActionResult> RejectNegotiationForConsignSaleLineItem([FromRoute] Guid consignLineItemId)
    {
        var result =
            await _consignSaleService.RejectNegotiation(consignLineItemId);

        return result.ResultStatus != ResultStatus.Success
            ? StatusCode((int)HttpStatusCode.InternalServerError, result)
            : Ok(result);
    }
    [Authorize(Roles = "Staff")]
    [HttpPost("{consignLineItemId}/create-individual-after-negotiation")]
    [ProducesResponseType<BusinessObjects.Dtos.Commons.Result<ConsignSaleLineItemResponse>>((int)HttpStatusCode.OK)]
    [ProducesResponseType<ErrorResponse>((int)HttpStatusCode.InternalServerError)]
    public async Task<IActionResult> CreateIndividualAfterNegotiation([FromRoute] Guid consignLineItemId, [FromBody] CreateIndividualAfterNegotiationRequest request)
    {
        var result =
            await _consignSaleService.CreateIndividualAfterNegotiation(consignLineItemId, request);

        return result.ResultStatus != ResultStatus.Success
            ? StatusCode((int)HttpStatusCode.InternalServerError, result)
            : Ok(result);
    }
    [Authorize]
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
    [Authorize(Roles = "Staff")]
    [HttpPost("{consignLineItemId}/create-masteritem")]
    [ProducesResponseType<BusinessObjects.Dtos.Commons.Result<MasterItemResponse>>((int)HttpStatusCode.OK)]
    [ProducesResponseType<ErrorResponse>((int)HttpStatusCode.InternalServerError)]
    public async Task<IActionResult> CreateMasterItemFromConsignSaleLineItem([FromRoute] Guid consignLineItemId,
        [FromBody] CreateMasterItemForConsignRequest detailRequest)
    {
        var result = await _consignSaleService.CreateMasterItemFromConsignSaleLineItem(consignLineItemId, detailRequest);
        if (result.ResultStatus != ResultStatus.Success)
        {
            return StatusCode((int)HttpStatusCode.InternalServerError, result);
        }

        return Ok(result);
    }
}