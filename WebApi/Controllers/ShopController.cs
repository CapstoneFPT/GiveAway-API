using BusinessObjects.Dtos.Commons;
using BusinessObjects.Dtos.FashionItems;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Services.FashionItems;

namespace WebApi.Controllers
{
    [Route("api/shops")]
    [ApiController]
    public class ShopController : ControllerBase
    {
        private readonly IFashionItemService _fashionItemService;

        public ShopController(IFashionItemService fashionItemService)
        {
            _fashionItemService = fashionItemService;
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
    }
}
