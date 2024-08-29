using System.Net;
using BusinessObjects.Dtos.Commons;
using BusinessObjects.Dtos.OrderLineItems;
using BusinessObjects.Dtos.Orders;
using BusinessObjects.Entities;
using Microsoft.AspNetCore.Mvc;
using Services.OrderLineItems;
using Services.Orders;

namespace WebApi.Controllers;

[ApiController]
[Route("api/orderlineitems")]
public class OrderLineItemController : ControllerBase
{
    private readonly IOrderService _orderService;
    private readonly IOrderLineItemService _orderLineItemService;

    public OrderLineItemController(IOrderService orderService, IOrderLineItemService orderLineItemService)
    {
        _orderService = orderService;
        _orderLineItemService = orderLineItemService;
    }

    [HttpPut("{orderLineItemId}/confirm-pending-order")]
    [ProducesResponseType<Result<OrderResponse>>((int)HttpStatusCode.OK)]
    [ProducesResponseType<ErrorResponse>((int)HttpStatusCode.InternalServerError)]
    public async Task<IActionResult> ConfirmPendingOrderLineItemByShop(
        [FromRoute] Guid orderLineItemId, FashionItemStatus itemStatus)
    {
        var result = await _orderService.ConfirmPendingOrder(orderLineItemId, itemStatus);
        if (result.ResultStatus != ResultStatus.Success)
            return StatusCode((int)HttpStatusCode.InternalServerError, result);
        return Ok(result);
    }

    [HttpGet("{orderLineItemId}")]
    public async Task<ActionResult<Result<OrderLineItemResponse<IndividualFashionItem>>>> GetOrderLineItemById(
        [FromRoute] Guid orderLineItemId)
    {
        var result = await _orderLineItemService.GetOrderLineItemById(orderLineItemId);

        if (result.ResultStatus != ResultStatus.Success)
            return StatusCode((int)HttpStatusCode.InternalServerError, result);


        return Ok(result);
    }
}