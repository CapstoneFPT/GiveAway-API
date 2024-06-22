using BusinessObjects.Dtos.Commons;
using BusinessObjects.Dtos.Order;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Services.Orders;

namespace WebApi.Controllers
{
    [Route("api/orders")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public OrderController(IOrderService orderService)
        {
            _orderService = orderService;
        }
        [HttpGet("accounts/{accountId}")]
        public async Task<ActionResult<Result<PaginationResponse<OrderResponse>>>> GetOrdersByAccountId([FromRoute] Guid accountId, [FromQuery] OrderRequest request)
        {
            return await _orderService.GetOrdersByAccountId(accountId, request);
        }
    }
}
