﻿using BusinessObjects.Dtos.Commons;
using BusinessObjects.Dtos.Shops;
using Repositories.Shops;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using BusinessObjects.Dtos.Inquiries;
using BusinessObjects.Entities;
using LinqKit;
using Microsoft.EntityFrameworkCore;
using Repositories.Inquiries;

namespace Services.Shops
{
    public class ShopService : IShopService
    {
        private readonly IShopRepository _shopRepository;
        private readonly IInquiryRepository _inquiryRepository;

        public ShopService(IShopRepository shopRepository, IInquiryRepository inquiryRepository)
        {
            _shopRepository = shopRepository;
            _inquiryRepository = inquiryRepository;
        }

        public async Task<Result<List<ShopDetailResponse>>> GetAllShop()
        {
            var response = new Result<List<ShopDetailResponse>>();
            var result = await _shopRepository.GetAllShop();
            if (result.Count != 0)
            {
                response.Data = result;
                response.Messages = ["Successfully"];
                response.ResultStatus = ResultStatus.Success;
                return response;
            }

            response.Messages = ["There isn't any shop available"];
            response.ResultStatus = ResultStatus.Empty;
            return response;
        }

        public async Task<Result<ShopDetailResponse>> GetShopById(Guid shopid)
        {
            var response = new Result<ShopDetailResponse>();
            var result = await _shopRepository.GetShopById(shopid);
            if (result != null)
            {
                response.Data = result;
                response.Messages = ["Successfully"];
                response.ResultStatus = ResultStatus.Success;
                return response;
            }

            response.Messages = ["There isn't any shop available"];
            response.ResultStatus = ResultStatus.NotFound;
            return response;
        }

        public async Task<PaginationResponse<InquiryListResponse>> GetInquiriesByShopId(Guid shopId,
            InquiryListRequest inquiryRequest)
        {
            Expression<Func<Inquiry, bool>> predicate = inquiry => inquiry.ShopId == shopId;

            if (!string.IsNullOrEmpty(inquiryRequest.Fullname))
            {
                predicate = predicate.And(inquiry => EF.Functions.ILike(inquiry.Fullname, $"%{inquiryRequest.Fullname}%"));
            }

            Expression<Func<Inquiry, InquiryListResponse>> selector = inquiry => new InquiryListResponse()
            {
                InquiryId = inquiry.InquiryId,
                ShopId = inquiry.ShopId,
                Fullname = inquiry.Fullname,
                Message = inquiry.Message,
                CreatedDate = inquiry.CreatedDate
            };

            (List<InquiryListResponse> Items, int Page, int PageSize, int TotalCount) result =
                await _inquiryRepository.GetInquiries<InquiryListResponse>(inquiryRequest.Page, inquiryRequest.PageSize,
                    predicate, selector);

            return new PaginationResponse<InquiryListResponse>()
            {
                Items = result.Items,
                PageNumber = result.Page,
                PageSize = result.PageSize,
                TotalCount = result.TotalCount
            };
        }
    }
}