using System.Net;
using AutoMapper;
using BusinessObjects.Dtos.AuctionItems;
using BusinessObjects.Dtos.Category;
using BusinessObjects.Dtos.Commons;
using BusinessObjects.Dtos.FashionItems;
using BusinessObjects.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
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
       
        /*[HttpGet("{categoryId}/fahsionitems")]
        public async Task<ActionResult<Result<PaginationResponse<FashionItemDetailResponse>>>> GetItemsByCategoryHierarchy([FromRoute] Guid categoryId, [FromQuery] AuctionFashionItemRequest request)
        {
            var result = await _fashionitemService.GetItemByCategoryHierarchy(categoryId, request);

            if (result.ResultStatus != ResultStatus.Success)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, result);
            }
            
            return Ok(result);
        }*/
        /*[HttpGet]
        public async Task<ActionResult<Result<List<Category>>>> GetAllParentCategory()
        {
            var result = await _categoryService.GetAllParentCategory();
            
            if (result.ResultStatus != ResultStatus.Success)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, result);
            }
            
            return Ok(result);
        }*/

        [HttpGet("tree")]
        public async Task<ActionResult<CategoryTreeResult>> GetTree([FromQuery] Guid? shopId, [FromQuery] Guid? rootCategoryId, [FromQuery] bool? isAvailable)
        {
            var result = await _categoryService.GetTree(shopId, rootCategoryId, isAvailable);
            return Ok(new CategoryTreeResult()
            {
                ShopId = shopId,
                Categories = result
            });
        }

        
        [HttpGet("{categoryId}")]
        public async Task<ActionResult<Result<List<Category>>>> GetAllChildrenCategory([FromRoute] Guid categoryId)
        {
            var result = await _categoryService.GetAllChildrenCategory(categoryId);
            
            if (result.ResultStatus != ResultStatus.Success)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, result);
            }
            
            return Ok(result);
        }

        [HttpPost("{categoryId}")]
        public async Task<ActionResult<Result<Category>>> CreateCategory([FromRoute] Guid categoryId, [FromBody] CreateCategoryRequest request)
        {
            var result = await _categoryService.CreateCategory(categoryId, request);
            
            if (result.ResultStatus != ResultStatus.Success)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, result);
            }
            
            return Ok(result);
        }

        [HttpGet("condition")]
        public async Task<ActionResult<Result<List<Category>>>> GetCategoryWithCondition(
            [FromQuery] CategoryRequest categoryRequest)
        {
            var result = await _categoryService.GetCategoryWithCondition(categoryRequest);
            
            if (result.ResultStatus != ResultStatus.Success)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, result);
            }
            
            return Ok(result);
        }
        [Authorize(Roles = "Admin")]
        [HttpPut("{categoryId}")]
        [ProducesResponseType<CategoryResponse>((int)HttpStatusCode.OK)]
        [ProducesResponseType<ErrorResponse>((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> UpdateNameCategory([FromRoute] Guid categoryId, UpdateCategoryRequest request)
        {
            var result = await _categoryService.UpdateNameCategory(categoryId, request);
            return result.ResultStatus != ResultStatus.Success ? StatusCode((int)HttpStatusCode.InternalServerError, result) : Ok(result);
        }
        [Authorize(Roles = "Admin")]
        [HttpPut("{categoryId}/status")]
        [ProducesResponseType<CategoryResponse>((int)HttpStatusCode.OK)]
        [ProducesResponseType<ErrorResponse>((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> UpdateStatusCategory([FromRoute] Guid categoryId)
        {
            var result = await _categoryService.UpdateStatusCategory(categoryId);
            return result.ResultStatus != ResultStatus.Success ? StatusCode((int)HttpStatusCode.InternalServerError, result) : Ok(result);
        }
    }

    public class CategoryTreeResult
    {
        public Guid? ShopId { get; set; }
        public List<CategoryTreeNode> Categories { get; set; }
    }
}
