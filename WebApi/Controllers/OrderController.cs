using BusinessObjects.Dtos.Commons;
using BusinessObjects.Dtos.FashionItems;
using BusinessObjects.Dtos.OrderDetails;
using BusinessObjects.Dtos.Orders;
using BusinessObjects.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Services.OrderDetails;
using Services.Orders;

namespace WebApi.Controllers
{
    [Route("api/orders")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _orderService;
        private readonly IOrderDetailService _orderDetailService;

        public OrderController(IOrderService orderService, IOrderDetailService orderDetailService)
        {
            _orderService = orderService;
            _orderDetailService = orderDetailService;
        }
        
        [HttpGet("{OrderId}/orderdetails")]
        public async Task<ActionResult<Result<PaginationResponse<OrderDetailResponse<FashionItem>>>>> GetOrderDetailsByOrderId([FromRoute] Guid OrderId, [FromQuery] OrderDetailRequest request)
        {
            return await _orderDetailService.GetOrderDetailsByOrderId(OrderId, request);
        }
        [HttpGet("{OrderId}/orderdetails/{OrderdetailId}")]
        public async Task<ActionResult<Result<OrderDetailResponse<FashionItem>>>> GetOrderDetailById([FromRoute] Guid OrderdetailId)
        {
            return await _orderDetailService.GetOrderDetailById(OrderdetailId);
        }
        
        [HttpPut("{OrderId}/cancel")]
        public async Task<ActionResult<Result<string>>> CancelOrder([FromRoute] Guid OrderId)
        {
            return await _orderService.CancelOrder(OrderId);
        }
        [HttpPut("{OrderId}/confirm-deliveried")]
        public async Task<ActionResult<Result<string>>> ConfirmOrder([FromRoute] Guid OrderId)
        {
            return await _orderService.ConfirmOrderDeliveried(OrderId);
        }
    }
}
