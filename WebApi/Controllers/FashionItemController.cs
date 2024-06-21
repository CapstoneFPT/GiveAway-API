﻿using BusinessObjects.Dtos.AuctionDeposits;
using BusinessObjects.Dtos.AuctionItems;
using BusinessObjects.Dtos.Commons;
using BusinessObjects.Dtos.FashionItems;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Services.FashionItems;

namespace WebApi.Controllers
{
    [Route("api/fashionitems")]
    [ApiController]
    public class FashionItemController : ControllerBase
    {
        private readonly IFashionItemService _fashionItemService;

        public FashionItemController(IFashionItemService fashionItemService)
        {
            _fashionItemService = fashionItemService;
        }
        [HttpGet]
        public async Task<ActionResult<Result<PaginationResponse<FashionItemDetailResponse>>>> GetAllFashionItemsPagination([FromQuery] AuctionFashionItemRequest request)
        {
            return await _fashionItemService.GetAllFashionItemPagination(request);
        }
        [HttpGet("{id}")]
        public async Task<ActionResult<Result<FashionItemDetailResponse>>> GetFashionItemById([FromRoute] Guid id)
        {
            return await _fashionItemService.GetFashionItemById(id);
        }

    }
}
