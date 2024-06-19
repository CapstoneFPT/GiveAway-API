using BusinessObjects.Dtos.AuctionItems;
using BusinessObjects.Dtos.Commons;
using BusinessObjects.Dtos.FashionItems;
using BusinessObjects.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Services.FashionItems;

namespace WebApi.Controllers
{
    [Route("api/categories")]
    [ApiController]
    public class CategoryController : ControllerBase
    {
        private readonly IFashionItemService _fashionitemService;

        public CategoryController(IFashionItemService fashionitemService)
        {
            _fashionitemService = fashionitemService;
        }
        [HttpGet("{categoryId}")]
        public async Task<ActionResult<Result<PaginationResponse<FashionItemDetailResponse>>>> GetItemsByCategoryHierarchy([FromRoute] Guid categoryId, [FromQuery] AuctionFashionItemRequest request)
        {
            return await _fashionitemService.GetItemByCategoryHierarchy(categoryId, request);
        }
    }
}
