using BusinessObjects.Dtos.Commons;
using BusinessObjects.Dtos.ConsignSales;
using BusinessObjects.Dtos.FashionItems;
using BusinessObjects.Dtos.Orders;
using BusinessObjects.Dtos.Shops;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Services.ConsignSales;
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
        private readonly IConsignSaleService _consignSaleService;

        public ShopController(IFashionItemService fashionItemService, IShopService shopService, IOrderService orderService, IConsignSaleService consignSaleService)
        {
            _fashionItemService = fashionItemService;
            _shopService = shopService;
            _orderService = orderService;
            _consignSaleService = consignSaleService;
        }

        [HttpPost("{shopId}/fashionitems")]
        public async Task<ActionResult<Result<FashionItemDetailResponse>>> AddFashionItem([FromRoute] Guid shopId, [FromBody] FashionItemDetailRequest request)
        {
            return await _fashionItemService.AddFashionItem(shopId, request);
        }
        
        
        [HttpGet]
        public async Task<ActionResult<Result<List<ShopDetailResponse>>>> GetAllShop()
        {
            return await _shopService.GetAllShop();
        }
        [HttpGet("{shopId}")]
        public async Task<ActionResult<Result<ShopDetailResponse>>> GetShopById([FromRoute] Guid shopId)
        {
            return await _shopService.GetShopById(shopId);
        }
        [HttpGet("{shopId}/orders")]
        public async Task<ActionResult<Result<PaginationResponse<OrderResponse>>>> GetOrdersByShopId([FromRoute] Guid shopId, [FromQuery] OrderRequest orderRequest)
        {
            return await _orderService.GetOrdersByShopId(shopId, orderRequest);
        }
        [HttpPost("{shopId}/orders")]
        public async Task<ActionResult<Result<OrderResponse>>> CreateOrderByShop([FromRoute] Guid shopId, [FromBody] CreateOrderRequest orderRequest)
        {
            return await _orderService.CreateOrderByShop(shopId, orderRequest);
        }
        [HttpPut("{shopId}/orders/{OrderId}/confirm-deliveried")]
        public async Task<ActionResult<Result<OrderResponse>>> ConfirmOrderDelivered([FromRoute] Guid shopId,[FromRoute] Guid OrderId)
        {
            return await _orderService.ConfirmOrderDeliveried(shopId,OrderId);
        }
        [HttpGet("{shopId}/consignsales")]
        public async Task<ActionResult<Result<PaginationResponse<ConsignSaleResponse>>>> GetAllConsignSaleByShopId([FromRoute] Guid shopId, [FromQuery] ConsignSaleRequest request)
        {
            return await _consignSaleService.GetAllConsignSales(shopId, request);
        }
        [HttpPost("{shopId}/consignsales")]
        public async Task<ActionResult<Result<ConsignSaleResponse>>> CreateConsignSaleByShop([FromRoute] Guid shopId, [FromBody] CreateConsignSaleByShopRequest orderRequest)
        {
            return await _consignSaleService.CreateConsignSaleByShop(shopId, orderRequest);
        }
        
    }
}
