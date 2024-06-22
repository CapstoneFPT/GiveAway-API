﻿using BusinessObjects.Dtos.AuctionItems;
using BusinessObjects.Dtos.Commons;
using BusinessObjects.Dtos.FashionItems;
using BusinessObjects.Entities;
using Dao;
using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Asn1.Ocsp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace Repositories.FashionItems
{
    public class FashionItemRepository : IFashionItemRepository
    {
        private readonly GenericDao<FashionItem> _fashionitemDao;
        private readonly GenericDao<Category> _categoryDao;

        public FashionItemRepository(GenericDao<FashionItem> fashionitemDao, GenericDao<Category> categoryDao)
        {
            _fashionitemDao = fashionitemDao;
            _categoryDao = categoryDao;
        }

        public async Task<FashionItem> AddFashionItem(FashionItem request)
        {
            return await _fashionitemDao.AddAsync(request);
        }

        public async Task<PaginationResponse<FashionItemDetailResponse>> GetAllFashionItemPagination(
            AuctionFashionItemRequest request)
        {
            try
            {
                var query = _fashionitemDao.GetQueryable();
                if (!string.IsNullOrWhiteSpace(request.SearchTerm))
                    query = query.Where(x => EF.Functions.ILike(x.Name, $"%{request.SearchTerm}%"));
                if (request.Status != null)
                {
                    query = query.Where(f => f.Status == request.Status);
                }

                if (!string.IsNullOrWhiteSpace(request.Type))
                {
                    query = query.Where(f => EF.Functions.ILike(f.Type, $"%{request.Type}%"));
                }

                if (request.ShopId != null)
                {
                    query = query.Where(f => f.ShopId.Equals(request.ShopId));
                }

                var count = await query.CountAsync();
                query = query.Skip((request.PageNumber - 1) * request.PageSize)
                    .Take(request.PageSize);

                var items = await query
                    
                    .Select(x => new FashionItemDetailResponse
                    {
                        ItemId = x.ItemId,
                        Type = x.Type,
                        Name = x.Name,
                        SellingPrice = x.SellingPrice,
                        Note = x.Note,
                        Quantity = x.Quantity,
                        Value = x.Value,
                        Condition = x.Condition,
                        ConsignDuration = x.ConsignSaleDetail.ConsignSale.ConsignDuration,
                        StartDate = x.ConsignSaleDetail.ConsignSale.StartDate,
                        EndDate = x.ConsignSaleDetail.ConsignSale.EndDate,
                        ShopAddress = x.Shop.Address,
                        ShopId = x.Shop.ShopId,
                        Consigner = x.ConsignSaleDetail.ConsignSale.Member.Fullname,
                        CategoryName = x.Category.Name,
                        Status = x.Status,
                    })
                    .AsNoTracking().ToListAsync();

                var result = new PaginationResponse<FashionItemDetailResponse>
                {
                    Items = items,
                    PageSize = request.PageSize,
                    TotalCount = count,
                    SearchTerm = request.SearchTerm,
                    PageNumber = request.PageNumber,
                };
                return result;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<FashionItem> GetFashionItemById(Guid id)
        {
            try
            {
                var query = await _fashionitemDao.GetQueryable()
                    .Include(c => c.Shop)
                    .Include(a => a.Category)
                    .Include(b => b.ConsignSaleDetail).ThenInclude(c => c.ConsignSale).ThenInclude(c => c.Member)
                    .AsNoTracking().FirstOrDefaultAsync(x => x.ItemId.Equals(id));
                return query;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<PaginationResponse<FashionItemDetailResponse>> GetItemByCategoryHierarchy(Guid id,
            AuctionFashionItemRequest request)
        {
           
            var listCate = new List<Guid>();
            await GetCategoryIdsRecursive(id, listCate);
            if (listCate.Count == 0)
            {
                return new PaginationResponse<FashionItemDetailResponse>
                {
                    Items = new List<FashionItemDetailResponse>(),
                    PageSize = request.PageSize,
                    TotalCount = 0,
                    SearchTerm = request.SearchTerm,
                    PageNumber = request.PageNumber,
                };
            }

            var query = _categoryDao.GetQueryable()
                .Where(c => listCate.Contains(c.CategoryId))
                .SelectMany(c => c.FashionItems)
                .Include((c => c.Shop))
                .Select(f => new FashionItemDetailResponse
                {
                    ItemId = f.ItemId,
                    Type = f.Type,
                    SellingPrice = f.SellingPrice,
                    Name = f.Name,
                    Note = f.Note,
                    Quantity = f.Quantity,
                    Value = f.Value,
                    Condition = f.Condition,
                    ConsignDuration = f.ConsignSaleDetail.ConsignSale.ConsignDuration,
                    Status = f.Status,
                    StartDate = f.ConsignSaleDetail.ConsignSale.StartDate,
                    EndDate = f.ConsignSaleDetail.ConsignSale.EndDate,
                    ShopAddress = f.Shop.Address,
                    ShopId = f.Shop.ShopId,
                    Consigner = f.ConsignSaleDetail.ConsignSale.Member.Fullname,
                    CategoryName = f.Category.Name,
                })
                .AsNoTracking();

            if (request.Status != null)
            {
                query = query.Where(f => f.Status == request.Status);
            }

            if (!string.IsNullOrWhiteSpace(request.Type))
            {
                query = query.Where(f => EF.Functions.ILike(f.Type, $"%{request.Type}%"));
            }

            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                query = query.Where(f => f.Name.Contains(request.SearchTerm));
            }

            if (request.ShopId != null)
            {
                query = query.Where(f => f.ShopId.Equals(request.ShopId));
            }

            var count = await query.CountAsync();

            var listitem = await query
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync();

            var result = new PaginationResponse<FashionItemDetailResponse>
            {
                Items = listitem,
                PageSize = request.PageSize,
                TotalCount = count,
                SearchTerm = request.SearchTerm,
                PageNumber = request.PageNumber,
            };


            return result;
        }

        public async Task<FashionItem> UpdateFashionItem(FashionItem fashionItem)
        {
            return await _fashionitemDao.UpdateAsync(fashionItem);
        }

        private async Task GetCategoryIdsRecursive(Guid? id, List<Guid> categoryIds)
        {
            var parentCate = await _categoryDao.GetQueryable().FirstOrDefaultAsync(c => c.CategoryId == id);
            if (parentCate == null) return;

            var listCate = await _categoryDao.GetQueryable()
                .Where(c => c.ParentId == id && c.Status.Equals(CategoryStatus.Available))
                .Select(c => c.CategoryId)
                .ToListAsync();
            categoryIds.AddRange(listCate);

            foreach (var childId in listCate)
            {
                await GetCategoryIdsRecursive(childId, categoryIds);
            }
        }
    }
}