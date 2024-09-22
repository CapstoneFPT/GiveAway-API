using System.Net;
using BusinessObjects.Dtos.Commons;
using BusinessObjects.Dtos.OrderLineItems;
using BusinessObjects.Dtos.Orders;
using BusinessObjects.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services.OrderLineItems;
using Services.Orders;

namespace WebApi.Controllers;
[Authorize]
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
    [Authorize(Roles = "Staff")]
    [HttpPut("{orderLineItemId}/confirm-pending-order")]
    [ProducesResponseType<Result<OrderResponse>>((int)HttpStatusCode.OK)]
    [ProducesResponseType<ErrorResponse>((int)HttpStatusCode.InternalServerError)]
    public async Task<IActionResult> ConfirmPendingOrderLineItemByShop(
        [FromRoute] Guid orderLineItemId, [FromBody] ConfirmPendingOrderRequest itemStatus)
    {
        var result = await _orderService.ConfirmPendingOrder(orderLineItemId, itemStatus);
        if (result.ResultStatus != ResultStatus.Success)
            return StatusCode((int)HttpStatusCode.InternalServerError, result);
        return Ok(result);
    }

    [HttpGet("{orderLineItemId}")]
    [ProducesResponseType<OrderLineItemDetailedResponse>((int) HttpStatusCode.OK)]
    [ProducesResponseType<ErrorResponse>((int) HttpStatusCode.InternalServerError)]
    public async Task<IActionResult> GetOrderLineItemById(
        [FromRoute] Guid orderLineItemId)
    {
        var result = await _orderLineItemService.GetOrderLineItemById(orderLineItemId);

        if (!result.IsSuccessful)
        {
            return result.Error switch
            {
                _ => StatusCode(500, new ErrorResponse("Error fetching order line item", ErrorType.ApiError,
                    HttpStatusCode.InternalServerError
                    , result.Error))
            };
        }

        return Ok(result.Value);
    }
}