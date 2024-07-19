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

        [HttpGet("tree")]
        public async Task<ActionResult<CategoryTreeResult>> GetTree([FromQuery] Guid? shopId, [FromQuery] Guid? rootCategoryId)
        {
            var result = await _categoryService.GetTree(shopId, rootCategoryId);
            return Ok(new CategoryTreeResult()
            {
                ShopId = shopId,
                Categories = result
            });
        }

        [HttpGet("leaves")]
        public async Task<ActionResult<CategoryLeavesResponse>> GetLeaves([FromQuery] Guid? shopId)
        {
           var result = await _categoryService.GetLeaves(shopId);
           return Ok(result);
        }
        [HttpGet("{categoryId}")]
        public async Task<ActionResult<Result<List<Category>>>> GetAllChildrenCategory([FromRoute] Guid categoryId)
        {
            return await _categoryService.GetAllChildrenCategory(categoryId);
        }

        [HttpPost("{categoryId}")]
        public async Task<ActionResult<Result<Category>>> CreateCategory([FromRoute] Guid categoryId, [FromBody] CreateCategoryRequest request)
        {
            return await _categoryService.CreateCategory(categoryId, request);
        }

        [HttpGet("condition")]
        public async Task<ActionResult<Result<List<Category>>>> GetCategoryWithCondition(
            [FromQuery] CategoryRequest categoryRequest)
        {
            return await _categoryService.GetCategoryWithCondition(categoryRequest);
        }
    }

    public class CategoryTreeResult
    {
        public Guid? ShopId { get; set; }
        public List<CategoryTreeNode> Categories { get; set; }
    }
}
