using BusinessObjects.Dtos.Commons;
using BusinessObjects.Dtos.FashionItems;
using BusinessObjects.Dtos.Shops;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Services.FashionItems;
using Services.Shops;

namespace WebApi.Controllers
{
    [Route("api/shops")]
    [ApiController]
    public class ShopController : ControllerBase
    {
        private readonly IFashionItemService _fashionItemService;
        private readonly IShopService _shopService;

        public ShopController(IFashionItemService fashionItemService, IShopService shopService)
        {
            _fashionItemService = fashionItemService;
            _shopService = shopService;
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
    }
}
