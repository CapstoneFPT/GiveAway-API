using BusinessObjects.Dtos.Commons;
using BusinessObjects.Dtos.FashionItems;
using BusinessObjects.Dtos.Orders;
using BusinessObjects.Dtos.Shops;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Services.FashionItems;
using Services.Orders;
using Services.Shops;

namespace WebApi.Controllers
{
    [Route("api/shops")]
    [ApiController]
    public class ShopController : ControllerBase
    {
        private readonly IFashionItemService _fashionItemService;
        private readonly IShopService _shopService;
        private readonly IOrderService _orderService;

        public ShopController(IFashionItemService fashionItemService, IShopService shopService, IOrderService orderService)
        {
            _fashionItemService = fashionItemService;
            _shopService = shopService;
            _orderService = orderService;
        }

        [HttpPost("{shopId}/fashionitems")]
        public async Task<ActionResult<Result<FashionItemDetailResponse>>> AddFashionItem([FromRoute] Guid shopId, [FromBody] FashionItemDetailRequest request)
        {
            return await _fashionItemService.AddFashionItem(shopId, request);
        }
        [HttpPut("{shopId}/fashionitems/{itemId}")]
        public async Task<ActionResult<Result<FashionItemDetailResponse>>> UpdateFashionItem([FromRoute] Guid itemId, [FromRoute] Guid shopId, [FromBody] FashionItemDetailRequest request)
        {
            return await _fashionItemService.UpdateFashionItem(itemId, shopId, request);
        }
        [HttpGet]
        public async Task<ActionResult<Result<List<ShopDetailResponse>>>> GetAllShop()
        {
            return await _shopService.GetAllShop();
        }
        [HttpGet("{shopId}/orders")]
        public async Task<ActionResult<Result<PaginationResponse<OrderResponse>>>> GetOrdersByShopId([FromRoute] Guid shopId, [FromQuery] OrderRequest orderRequest)
        {
            return await _orderService.GetOrdersByShopId(shopId, orderRequest);
        }
    }
}
