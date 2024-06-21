using AutoMapper;
using BusinessObjects.Dtos.AuctionItems;
using BusinessObjects.Dtos.Category;
using BusinessObjects.Dtos.Commons;
using BusinessObjects.Dtos.FashionItems;
using BusinessObjects.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Services.Categories;
using Services.FashionItems;

namespace WebApi.Controllers
{
    [Route("api/categories")]
    [ApiController]
    public class CategoryController : ControllerBase
    {
        private readonly IFashionItemService _fashionitemService;
        private readonly ICategoryService _categoryService;

        public CategoryController(IFashionItemService fashionitemService, ICategoryService categoryService)
        {
            _fashionitemService = fashionitemService;
            _categoryService = categoryService;
        }
        [HttpGet("{categoryId}/fahsionitems")]
        public async Task<ActionResult<Result<PaginationResponse<FashionItemDetailResponse>>>> GetItemsByCategoryHierarchy([FromRoute] Guid categoryId, [FromQuery] AuctionFashionItemRequest request)
        {
            return await _fashionitemService.GetItemByCategoryHierarchy(categoryId, request);
        }
        [HttpGet]
        public async Task<ActionResult<Result<List<Category>>>> GetAllParentCategory()
        {
            return await _categoryService.GetAllParentCategory();
        }
        [HttpGet("{categoryId}")]
        public async Task<ActionResult<Result<List<Category>>>> GetAllChildrenCategory([FromRoute] Guid categoryId)
        {
            return await _categoryService.GetAllChildrenCategory(categoryId);
        }

        [HttpPost("{parentId}")]
        public async Task<ActionResult<Result<Category>>> CreateCategory([FromRoute] Guid parentId,[FromBody] CategoryRequest request)
        {
            return await _categoryService.CreateCategory(parentId, request);
        }
    }
}
