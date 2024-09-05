﻿using System.Linq.Expressions;
using AutoMapper;
using AutoMapper.Internal;
using BusinessObjects.Dtos.AuctionDeposits;
using BusinessObjects.Dtos.AuctionItems;
using BusinessObjects.Dtos.Auctions;
using BusinessObjects.Dtos.Commons;
using BusinessObjects.Dtos.FashionItems;
using BusinessObjects.Entities;
using BusinessObjects.Utils;
using DotNext;
using DotNext.Collections.Generic;
using LinqKit;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Repositories.Categories;
using Repositories.ConsignSales;
using Repositories.FashionItems;
using Repositories.Images;
using Repositories.Shops;

namespace Services.FashionItems
{
    public class FashionItemService : IFashionItemService
    {
        private readonly IFashionItemRepository _fashionitemRepository;
        private readonly IImageRepository _imageRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IMapper _mapper;
        private readonly IConsignSaleRepository _consignSaleRepository;
        private readonly IShopRepository _shopRepository;

        public FashionItemService(IFashionItemRepository fashionitemRepository, IImageRepository imageRepository,
            ICategoryRepository categoryRepository,
            IMapper mapper, IConsignSaleRepository consignSaleRepository, IShopRepository shopRepository)
        {
            _fashionitemRepository = fashionitemRepository;
            _imageRepository = imageRepository;
            _categoryRepository = categoryRepository;
            _mapper = mapper;
            _consignSaleRepository = consignSaleRepository;
            _shopRepository = shopRepository;
        }

        public async Task<BusinessObjects.Dtos.Commons.Result<FashionItemDetailResponse>> AddFashionItem(Guid shopId,
            FashionItemDetailRequest request)
        {
            var response = new BusinessObjects.Dtos.Commons.Result<FashionItemDetailResponse>();
            var item = new IndividualFashionItem();
            var newdata = new IndividualFashionItem()
            {
                Note = !string.IsNullOrEmpty(request.Note) ? request.Note : null,
                // ShopId = shopId,
                Type = FashionItemType.ItemBase,
                Status = FashionItemStatus.Unavailable,
                SellingPrice = request.SellingPrice,
            };

            var newItem = await _fashionitemRepository.AddInvidualFashionItem(newdata);
            foreach (string img in request.Images)
            {
                var newimage = new Image()
                {
                    Url = img,
                    IndividualFashionItemId = newItem.ItemId,
                    CreatedDate = DateTime.UtcNow
                };
                await _imageRepository.AddImage(newimage);
            }

            response.Data = _mapper.Map<FashionItemDetailResponse>(newItem);
            response.Messages = ["Add successfully"];
            response.ResultStatus = ResultStatus.Success;
            return response;
        }

        public async Task<PaginationResponse<FashionItemList>> GetAllFashionItemPagination(
            FashionItemListRequest request)
        {
            Expression<Func<IndividualFashionItem, bool>> predicate = x => true;
            Expression<Func<IndividualFashionItem, FashionItemList>> selector = item => new
                FashionItemList()
                {
                    ItemId = item.ItemId,
                    Name = item.MasterItem.Name,
                    Note = item.Note ?? string.Empty,
                    CategoryId = item.MasterItem.CategoryId,
                    Condition = item.Condition,
                    Brand = item.MasterItem.Brand,
                    Gender = item.MasterItem.Gender,
                    Size = item.Size,
                    Type = item.Type,
                    Status = item.Status,
                    Color = item.Color,
                    SellingPrice = item.SellingPrice ?? 0,
                    Image = item.Images.FirstOrDefault() != null ? item.Images.First().Url : string.Empty,
                    MasterItemId = item.MasterItemId,
                    ShopId = item.MasterItem.ShopId,
                    ItemCode = item.ItemCode,
                };

            if (!string.IsNullOrEmpty(request.Name))
            {
                predicate = predicate.And(item =>
                    item != null && EF.Functions.ILike(item.MasterItem.Name, $"%{request.Name}%"));
            }

            if (!string.IsNullOrEmpty(request.MasterItemCode))
            {
                predicate = predicate
                    .And(item =>
                        item != null &&
                        EF.Functions.ILike(item.MasterItem.MasterItemCode, $"%{request.MasterItemCode}%"));
            }

            if (!string.IsNullOrEmpty(request.ItemCode))
            {
                predicate = predicate.And(item => EF.Functions.ILike(item.ItemCode, $"%{request.ItemCode}%"));
            }


            if (request.Status.Length > 0)
            {
                predicate = predicate.And(item => request.Status.Contains(item.Status));
            }

            if (request.Type.Length > 0)
            {
                predicate = predicate.And(item => request.Type.Contains(item.Type));
            }

            if (request.CategoryId.HasValue)
            {
                var categoryIds = await _categoryRepository.GetAllChildrenCategoryIds(request.CategoryId.Value);
                predicate = predicate.And(item =>
                    item != null && categoryIds.Contains(item.MasterItem.CategoryId));
            }

            if (request.ShopId.HasValue)
            {
                predicate = predicate.And(item =>
                    item != null && item.MasterItem.ShopId == request.ShopId);
            }

            if (request.Gender.HasValue)
            {
                predicate = predicate.And(item =>
                    item != null && item.MasterItem.Gender == request.Gender);
            }

            if (request.MinPrice.HasValue)
            {
                predicate = predicate.And(item => item.SellingPrice >= request.MinPrice);
            }

            if (request.MaxPrice.HasValue)
            {
                predicate = predicate.And(item => item.SellingPrice <= request.MaxPrice);
            }

            if (request.Color != null)
            {
                predicate = predicate.And(item => item != null && item.Color == request.Color);
            }

            if (request.Size != null)
            {
                predicate = predicate.And(item => item != null && item.Size == request.Size);
            }

            if (request.MasterItemId != null)
            {
                predicate = predicate.And(item =>
                    item != null && item.MasterItemId == request.MasterItemId);
            }

            if (!string.IsNullOrEmpty(request.Condition))
            {
                predicate = predicate.And(item => item != null && item.Condition == request.Condition);
            }

            (List<FashionItemList> Items, int Page, int PageSize, int TotalCount) result =
                await _fashionitemRepository.GetIndividualItemProjections(request.PageNumber, request.PageSize,
                    predicate,
                    selector);


            if (request.SortBy != null)
            {
                result.Items = request.SortDescending
                    ? result.Items.OrderByDescending(x => x.GetType().GetProperty(request.SortBy)?.GetValue(x)).ToList()
                    : result.Items.OrderBy(x => x.GetType().GetProperty(request.SortBy)?.GetValue(x)).ToList();
            }

            await CheckFashionItemsInOrder(result.Items, request.MemberId);

            return new PaginationResponse<FashionItemList>()
            {
                Items = result.Items,
                PageNumber = result.Page,
                PageSize = result.PageSize,
                TotalCount = result.TotalCount,
                SearchTerm = request.ItemCode,
                OrderBy = request.SortBy,
            };
        }

        private async Task CheckFashionItemsInOrder(List<FashionItemList> items, Guid? memberId)
        {
            if (memberId.HasValue)
            {
                var itemsId = items.Select(x => x.ItemId).ToList();
                var orderItems = await _fashionitemRepository.GetOrderedItems(itemsId, memberId.Value);
                foreach (var item in items)
                {
                    item.IsOrderedYet = orderItems.Contains(item.ItemId);
                }
            }
        }

        public async Task<DotNext.Result<MasterItemDetailResponse, ErrorCode>> GetMasterItemById(Guid id)
        {
            var masterItem = await _fashionitemRepository.GetSingleMasterItem(x => x.MasterItemId == id);

            if (masterItem == null)
            {
                return new Result<MasterItemDetailResponse, ErrorCode>(ErrorCode.NotFound);
            }

            return new Result<MasterItemDetailResponse, ErrorCode>(
                new MasterItemDetailResponse()
                {
                    MasterItemId = masterItem.MasterItemId,
                    Brand = masterItem.Brand,
                    Gender = masterItem.Gender,
                    CategoryId = masterItem.CategoryId,
                    IsConsignment = masterItem.IsConsignment,
                    Description = masterItem.Description,
                    CreatedDate = masterItem.CreatedDate,
                    StockCount = masterItem.StockCount,
                    MasterItemCode = masterItem.MasterItemCode,
                    Name = masterItem.Name,
                    CategoryName = masterItem.Category.Name,
                    Images = masterItem.Images.Select(x => new FashionItemImage()
                    {
                        ImageId = x.ImageId,
                        ImageUrl = x.Url
                    }).ToList(),
                });
        }

        private async Task CheckItemsInOrder(List<IndividualItemListResponse> items, Guid? memberId)
        {
            if (memberId.HasValue)
            {
                var itemsId = items.Select(x => x.ItemId).ToList();

                var orderItems = await _fashionitemRepository.GetOrderedItems(itemsId, memberId.Value);
                foreach (var item in items)
                {
                    item.IsOrderedYet = orderItems.Contains(item.ItemId);
                }
            }
        }

        public async Task<BusinessObjects.Dtos.Commons.Result<FashionItemDetailResponse>> GetFashionItemById(Guid id,
            Guid? memberId)
        {
            var response = new BusinessObjects.Dtos.Commons.Result<FashionItemDetailResponse>();
            var item = await _fashionitemRepository.GetFashionItemById(c => c.ItemId == id);
            if (item == null)
            {
                response.Messages = ["Item is not existed"];
                response.ResultStatus = ResultStatus.NotFound;
                return response;
            }

            var result = new FashionItemDetailResponse()
            {
                ItemId = item.ItemId,
                Brand = item.MasterItem.Brand,
                Color = item.Color ?? "N/A",
                Condition = item.Condition ?? "N/A",
                Description = item.MasterItem.Description ?? "N/A",
                Gender = item.MasterItem.Gender,
                Name = item.MasterItem.Name,
                Size = item.Size,
                SellingPrice = item.SellingPrice ?? 0,
                Status = item.Status,
                Images = item.Images.Select(x => x.Url).ToList(),
                Note = item.Note,
                Type = item.Type,
                ShopAddress = item.MasterItem.Shop.Address,
                CategoryName = item.MasterItem.Category.Name,
                IsConsignment = item.MasterItem.IsConsignment,
                ItemCode = item.ItemCode,
                IsOrderedYet = memberId != null &&
                               _fashionitemRepository.CheckItemIsInOrder(item.ItemId, memberId.Value)
            };

            response.Data = result;
            response.ResultStatus = ResultStatus.Success;
            response.Messages = ["Successfully"];
            return response;
        }

        public async Task<BusinessObjects.Dtos.Commons.Result<FashionItemDetailResponse>> UpdateFashionItem(Guid itemId,
            UpdateFashionItemRequest request)
        {
            var response = new BusinessObjects.Dtos.Commons.Result<FashionItemDetailResponse>();
            var item = await _fashionitemRepository.GetFashionItemById(c => c.ItemId == itemId);
            if (item is null || item.MasterItem.IsConsignment)
            {
                throw new FashionItemNotFoundException();
            }

            item.Images.Clear();
            item.Images = request.imageUrls
                .Select(x => new Image()
                {
                    Url = x,
                    CreatedDate = DateTime.UtcNow,
                }).ToList();
            item.SellingPrice = request.SellingPrice ?? item.SellingPrice;
            item.Note = request.Note ?? item.Note;
            item.Color = request.Color ?? item.Color;
            item.Condition = request.Condition ?? item.Condition;
            item.Size = request.Size ?? item.Size;
            if (item.Images.Any() && item.Status == FashionItemStatus.Draft)
            {
                item.Status = FashionItemStatus.Unavailable;
            }
            await _fashionitemRepository.UpdateFashionItem(item);
            response.Data =
                _mapper.Map<FashionItemDetailResponse>(item);
            response.ResultStatus = ResultStatus.Success;
            response.Messages = ["Update Successfully"];
            return response;
        }

        public async Task<BusinessObjects.Dtos.Commons.Result<PaginationResponse<FashionItemDetailResponse>>>
            GetItemByCategoryHierarchy(
                Guid categoryId, AuctionFashionItemRequest request)
        {
            var response = new BusinessObjects.Dtos.Commons.Result<PaginationResponse<FashionItemDetailResponse>>();
            var items = await _fashionitemRepository.GetItemByCategoryHierarchy(categoryId, request);
            if (items.TotalCount > 0)
            {
                response.Data = items;
                response.ResultStatus = ResultStatus.Success;
                response.Messages = ["Successfully with " + response.Data.TotalCount + " items"];
                return response;
            }

            response.ResultStatus = ResultStatus.Success;
            response.Messages = ["Empty"];
            return response;
        }

        public async Task<BusinessObjects.Dtos.Commons.Result<FashionItemDetailResponse>> CheckFashionItemAvailability(
            Guid itemId)
        {
            var response = new BusinessObjects.Dtos.Commons.Result<FashionItemDetailResponse>();
            var item = await _fashionitemRepository.GetFashionItemById(c => c.ItemId == itemId);
            if (item is null || (item.Status != FashionItemStatus.Available &&
                                 item.Status != FashionItemStatus.Unavailable))
            {
                throw new FashionItemNotFoundException();
            }

            if (item.Status.Equals(FashionItemStatus.Unavailable))
            {
                item.Status = FashionItemStatus.Available;
                response.Messages = ["This item status has successfully changed to available"];
            }
            else
            {
                item.Status = FashionItemStatus.Unavailable;
                response.Messages = ["This item status has successfully changed to unavailable"];
            }

            response.Data = _mapper.Map<FashionItemDetailResponse>(item);
            response.ResultStatus = ResultStatus.Success;
            await _fashionitemRepository.UpdateFashionItem(item);

            return response;
        }

        public async Task<List<IndividualFashionItem>> GetRefundableItems()
        {
            Expression<Func<IndividualFashionItem, bool>> predicate = x => x.Status == FashionItemStatus.Refundable;
            var result = await _fashionitemRepository
                .GetFashionItems(predicate);
            return result;
        }

        public Task ChangeToSoldItems(List<IndividualFashionItem> refundableItems)
        {
            foreach (var item in refundableItems)
            {
                item.Status = FashionItemStatus.Sold;
            }

            return _fashionitemRepository.UpdateFashionItems(refundableItems);
        }

        public async Task<BusinessObjects.Dtos.Commons.Result<FashionItemDetailResponse?>> UpdateFashionItemStatus(
            Guid itemId,
            UpdateFashionItemStatusRequest request)
        {
            var item = await _fashionitemRepository.GetFashionItemById(c => c.ItemId == itemId);

            item.Status = request.Status;

            await _fashionitemRepository.UpdateFashionItem(item);
            return new BusinessObjects.Dtos.Commons.Result<FashionItemDetailResponse?>
            {
                Data = _mapper.Map<FashionItemDetailResponse>(item),
                ResultStatus = ResultStatus.Success,
                Messages = ["Update Successfully"]
            };
        }

        public async Task<BusinessObjects.Dtos.Commons.Result<List<MasterItemResponse>>> CreateMasterItemByAdmin(
            CreateMasterItemRequest masterItemRequest)
        {
            var listMasterItemResponse = new List<MasterFashionItem>();
            var listShopAvailable = await _shopRepository.GetShopEntities(c => c.StaffId != null);

            if (masterItemRequest.ItemForEachShops.Length == 0)
            {
                foreach (var shop in listShopAvailable)
                {
                    var masterItem = new MasterFashionItem()
                    {
                        Name = masterItemRequest.Name,
                        Gender = masterItemRequest.Gender,
                        Brand = masterItemRequest.Brand ?? "No Brand",
                        Description = masterItemRequest.Description,
                        MasterItemCode =
                            await _fashionitemRepository.GenerateMasterItemCode(masterItemRequest.MasterItemCode),
                        CategoryId = masterItemRequest.CategoryId,
                        StockCount = 0,
                        IsConsignment = false,
                        CreatedDate = DateTime.UtcNow,
                        ShopId = shop.ShopId
                    };
                    masterItem = await _fashionitemRepository.AddSingleMasterFashionItem(masterItem);

                    var imgForMaster = masterItemRequest.Images.Select(
                        image => new Image()
                        {
                            Url = image, CreatedDate = DateTime.UtcNow, MasterFashionItemId = masterItem.MasterItemId,
                        }).ToList();

                    await _imageRepository.AddRangeImage(imgForMaster);
                    masterItem.Images = imgForMaster;
                    listMasterItemResponse.Add(masterItem);
                }
            }
            else
            {
                foreach (var shop in masterItemRequest.ItemForEachShops!)
                {
                    var masterItem = new MasterFashionItem()
                    {
                        Name = masterItemRequest.Name,
                        Gender = masterItemRequest.Gender,
                        Brand = masterItemRequest.Brand ?? "No Brand",
                        Description = masterItemRequest.Description,
                        MasterItemCode =
                            await _fashionitemRepository.GenerateMasterItemCode(masterItemRequest.MasterItemCode),
                        CategoryId = masterItemRequest.CategoryId,
                        StockCount = 0,
                        IsConsignment = false,
                        CreatedDate = DateTime.UtcNow,
                        ShopId = shop.ShopId
                    };
                    masterItem = await _fashionitemRepository.AddSingleMasterFashionItem(masterItem);

                    var imgForMaster = masterItemRequest.Images.Select(
                        image => new Image()
                        {
                            Url = image, CreatedDate = DateTime.UtcNow, MasterFashionItemId = masterItem.MasterItemId,
                        }).ToList();

                    await _imageRepository.AddRangeImage(imgForMaster);
                    masterItem.Images = imgForMaster;
                    listMasterItemResponse.Add(masterItem);
                }
            }

            return new BusinessObjects.Dtos.Commons.Result<List<MasterItemResponse>>()
            {
                Data = _mapper.Map<List<MasterItemResponse>>(listMasterItemResponse),
                ResultStatus = ResultStatus.Success,
                Messages = new[] { "Add successfully" }
            };
        }

        public async Task<BusinessObjects.Dtos.Commons.Result<MasterItemResponse>> UpdateMasterItem(
            Guid masteritemId,
            UpdateMasterItemRequest masterItemRequest)
        {
            Expression<Func<MasterFashionItem, bool>> predicate = masterItem => masterItem.MasterItemId == masteritemId;
            var itemMaster = await _fashionitemRepository.GetSingleMasterItem(predicate);
            if (itemMaster is null)
                throw new MasterItemNotAvailableException("Can not find master item");
            if (masterItemRequest.CategoryId != null)
            {
                var category = await _categoryRepository.GetCategoryById(masterItemRequest.CategoryId.Value);
                if (category is null)
                {
                    throw new CategoryNotFound("Your new category is not found");
                }

                itemMaster.CategoryId = masterItemRequest.CategoryId.Value;
                itemMaster.Category = category;
            }

            
            itemMaster.Description = masterItemRequest.Description ?? itemMaster.Description;
            itemMaster.Name = masterItemRequest.Name ?? itemMaster.Name;
            itemMaster.Brand = masterItemRequest.Brand ?? itemMaster.Brand;

            itemMaster.Gender = masterItemRequest.Gender ?? itemMaster.Gender;
            

            itemMaster.Images.Clear();

            itemMaster.Images = masterItemRequest.ImageRequests
                .Select(x => new Image()
                {
                    Url = x,
                    CreatedDate = DateTime.UtcNow,
                }).ToList();

            await _fashionitemRepository.UpdateMasterItem(itemMaster);
            return new BusinessObjects.Dtos.Commons.Result<MasterItemResponse>()
            {
                Data = _mapper.Map<MasterItemResponse>(itemMaster),
                ResultStatus = ResultStatus.Success,
                Messages = new[] { "Update new master item successfully" }
            };
        }

        public async Task<BusinessObjects.Dtos.Commons.Result<List<IndividualItemListResponse>>> CreateIndividualItems(
            Guid masterItemId,
            CreateIndividualItemRequest request)
        {
            Expression<Func<MasterFashionItem, bool>> predicate = masterItem =>
                masterItem.MasterItemId == masterItemId;
            var masterItem = await _fashionitemRepository.GetSingleMasterItem(predicate);
            if (masterItem is null || masterItem.IsConsignment)
            {
                throw new MasterItemNotAvailableException("Master item is not found or unable to add items");
            }

            var listIndividualResponse = new List<IndividualFashionItem>();
            for (int i = 0; i < request.ItemInStock; i++)
            {
                var dataIndividual = new IndividualFashionItem()
                {
                    Note = request.Note,

                    Size = request.Size,
                    Color = request.Color,
                    Condition = request.Condition,
                    MasterItemId = masterItemId,
                    CreatedDate = DateTime.UtcNow,
                    Status = FashionItemStatus.Draft,
                    Type = FashionItemType.ItemBase,
                    SellingPrice = request.SellingPrice,
                    ItemCode = await _fashionitemRepository.GenerateIndividualItemCode(masterItem!.MasterItemCode),
                };
                await _fashionitemRepository.AddInvidualFashionItem(dataIndividual);
                listIndividualResponse.Add(dataIndividual);
            }

            return new BusinessObjects.Dtos.Commons.Result<List<IndividualItemListResponse>>()
            {
                Data = listIndividualResponse.Select(c => new IndividualItemListResponse()
                {
                    ItemId = c.ItemId,
                    MasterItemId = c.MasterItemId,
                    CreatedDate = c.CreatedDate,
                    Status = c.Status,
                    ItemCode = c.ItemCode,
                    SellingPrice = c.SellingPrice!.Value,
                    Color = c.Color,
                    Condition = c.Condition,
                    Size = c.Size,
                    Type = c.Type
                }).ToList(),
                ResultStatus = ResultStatus.Success,
                Messages = new[] { "Add items successfully" }
            };
        }

        public async Task<PaginationResponse<MasterItemListResponse>> GetAllMasterItemPagination(
            MasterItemRequest request)
        {
            Expression<Func<MasterFashionItem, bool>> predicate = x => true;
            Expression<Func<MasterFashionItem, MasterItemListResponse>> selector = item => new
                MasterItemListResponse()
                {
                    MasterItemId = item.MasterItemId,
                    Name = item.Name,
                    Description = item.Description ?? string.Empty,
                    ItemCode = item.MasterItemCode,
                    CreatedDate = item.CreatedDate,
                    Brand = item.Brand,
                    Gender = item.Gender,
                    CategoryId = item.CategoryId,
                    IsConsignment = item.IsConsignment,
                    ItemInStock = item.IndividualFashionItems.Count(c => c.Status == FashionItemStatus.Available),
                    ShopId = item.ShopId,
                    ShopAddress = item.Shop.Address,
                    StockCount = item.IndividualFashionItems.Count,
                    Images = item.Images.Select(x => x.Url).ToList()
                };
            (List<MasterItemListResponse> Items, int Page, int PageSize, int TotalCount) result =
                new ValueTuple<List<MasterItemListResponse>, int, int, int>();
            if (request.IsLeftInStock)
            {
                predicate = predicate.And(it =>
                    it.IndividualFashionItems.Any(c => c.Status == FashionItemStatus.Available));
            }

            if (!string.IsNullOrEmpty(request.SearchTerm))
            {
                predicate = predicate.And(item => EF.Functions.ILike(item.Name, $"%{request.SearchTerm}%"));
            }

            if (!string.IsNullOrEmpty(request.SearchItemCode))
            {
                predicate = predicate.And(item =>
                    EF.Functions.ILike(item.MasterItemCode, $"%{request.SearchItemCode.ToUpper()}%"));
            }

            if (request.CategoryId.HasValue)
            {
                var categoryIds = await _categoryRepository.GetAllChildrenCategoryIds(request.CategoryId.Value);
                predicate = predicate.And(item => categoryIds.Contains(item.CategoryId));
            }

            if (request.Brand != null)
            {
                predicate = predicate.And(item => EF.Functions.ILike(item.Brand, $"%{request.Brand}%"));
            }

            if (request.IsConsignment != null)
            {
                if (request.IsConsignment == true)
                {
                    predicate = predicate.And(item => item.IsConsignment == true);
                }
                else
                {
                    predicate = predicate.And(item => item.IsConsignment == false);
                }
            }

            if (request.ShopId.HasValue)
            {
                predicate = predicate.And(item => item.ShopId == request.ShopId);
            }

            if (request.GenderType.HasValue)
            {
                predicate = predicate.And(item => item.Gender == request.GenderType);
            }

            result = await _fashionitemRepository.GetMasterItemProjections(request.PageNumber, request.PageSize,
                predicate,
                selector);


            return new PaginationResponse<MasterItemListResponse>()
            {
                Items = result.Items!,
                PageNumber = result.Page,
                PageSize = result.PageSize,
                TotalCount = result.TotalCount
            };
        }

        public async Task<PaginationResponse<MasterItemListResponse>> GetMasterItemFrontPage(
            FrontPageMasterItemRequest request)
        {
            var query = _fashionitemRepository.GetMasterQueryable();

            if (!string.IsNullOrEmpty(request.SearchTerm))
            {
                query = query.Where(item => EF.Functions.ILike(item.Name, $"%{request.SearchTerm}%"));
            }

            if (request.CategoryId.HasValue)
            {
                var categoryIds = await _categoryRepository.GetAllChildrenCategoryIds(request.CategoryId.Value);
                query = query.Where(item => categoryIds.Contains(item.CategoryId));
            }

            if (request.GenderType.HasValue)
            {
                query = query.Where(item => item.Gender == request.GenderType);
            }
            if (request.IsLeftInStock)
            {
                query = query.Where(it =>
                    it.IndividualFashionItems.Any(c => c.Status == FashionItemStatus.Available));
            }
            var groupedQuery = query
                .GroupBy(m => new
                {
                    m.MasterItemCode,
                    m.Name,
                    m.Brand,
                    m.Gender,
                    m.CategoryId,
                    m.IsConsignment,
                })
                .Select(g => new MasterItemListResponse
                {
                    MasterItemId = g.First().MasterItemId,
                    Name = g.First().Name,
                    Description = g.First().Description ?? string.Empty,
                    ItemCode = g.First().MasterItemCode,
                    Brand = g.First().Brand,
                    Gender = g.First().Gender,
                    CategoryId = g.First().CategoryId,
                    IsConsignment = g.First().IsConsignment,
                    StockCount = g.Sum(c => c.IndividualFashionItems.Count()),
                    ItemInStock = g.Sum(c => c.IndividualFashionItems.Count(ind => ind.Status == FashionItemStatus.Available)),
                    Images = g.First().Images.Select(i => i.Url).ToList()
                });

            var totalCount = await groupedQuery.CountAsync();

            var items = await groupedQuery
                .Skip(((request.PageNumber ?? 1) - 1) * (request.PageSize ?? 10))
                .Take(request.PageSize ?? 10)
                .ToListAsync();

            return new PaginationResponse<MasterItemListResponse>
            {
                Items = items,
                PageNumber = request.PageNumber ?? 1,
                PageSize = request.PageSize ?? 10,
                TotalCount = totalCount
            };
        }

        public async Task<Result<MasterItemDetailResponse, ErrorCode>> FindMasterItem(FindMasterItemRequest request)
        {
            if (request.Name is null && request.MasterItemCode is null && request.MasterItemId == null)
            {
                return new Result<MasterItemDetailResponse, ErrorCode>(ErrorCode.NotFound);
            }

            var queryable = _fashionitemRepository.GetMasterQueryable();

            Expression<Func<MasterFashionItem, bool>> predicate = item => true;
            Expression<Func<MasterFashionItem, MasterItemDetailResponse>> selector = item =>
                new MasterItemDetailResponse()
                {
                    MasterItemId = item.MasterItemId,
                    Name = item.Name,
                    Description = item.Description,
                    MasterItemCode = item.MasterItemCode,
                    Brand = item.Brand,
                    Gender = item.Gender,
                    CategoryId = item.CategoryId,
                    IsConsignment = item.IsConsignment,
                    CreatedDate = item.CreatedDate,
                    CategoryName = item.Category.Name,
                    StockCount = item.StockCount,
                    Images = item.Images.Select(x => new FashionItemImage()
                    {
                        ImageId = x.ImageId,
                        ImageUrl = x.Url
                    }).ToList()
                };

            if (!string.IsNullOrEmpty(request.MasterItemCode))
            {
                predicate = predicate.And(item => item.MasterItemCode == request.MasterItemCode);
            }

            if (!string.IsNullOrEmpty(request.Name))
            {
                predicate = predicate.And(item => item.Name == request.Name);
            }

            if (request.MasterItemId.HasValue)
            {
                predicate = predicate.And(item => item.MasterItemId == request.MasterItemId);
            }

            var result = await queryable
                .Where(predicate)
                .Select(selector)
                .FirstOrDefaultAsync();

            return result == null
                ? new Result<MasterItemDetailResponse, ErrorCode>(ErrorCode.NotFound)
                : new Result<MasterItemDetailResponse, ErrorCode>(result);
        }

        public async Task<BusinessObjects.Dtos.Commons.Result<string?>> DeleteDraftItem(List<DeleteDraftItemRequest> deleteDraftItemRequests)
        {
            var itemIds = deleteDraftItemRequests.Select(x => x.ItemId).ToList();
            Expression<Func<IndividualFashionItem, bool>> predicate = individualItem =>
                itemIds.Contains(individualItem.ItemId);
            var listIndividual = await _fashionitemRepository.GetFashionItems(predicate);
            if (listIndividual.Any(c => c.Status != FashionItemStatus.Draft))
            {
                throw new ItemUnableToDeleteException("There are items unable to delete. You can only delete draft item");
            }

            var result = await _fashionitemRepository.DeleteRangeIndividualItems(listIndividual);
            if (result is false)
            {
                throw new DeleteFashionItemsFailedException("Delete failed by sever error", ErrorCode.ServerError);
            }

            return new BusinessObjects.Dtos.Commons.Result<string?>()
            {
                ResultStatus = ResultStatus.Success,
                Messages = new []{"Delete items successfully"}
            };
        }

        /*public async Task<PaginationResponse<ItemVariationListResponse>> GetAllFashionItemVariationPagination(
            Guid masterItemId, ItemVariationRequest request)
        {
            Expression<Func<FashionItemVariation, bool>> predicate = x => x.MasterItemId == masterItemId;
            Expression<Func<FashionItemVariation, ItemVariationListResponse>> selector = item =>
                new ItemVariationListResponse
                {
                    VariationId = item.VariationId,
                    CreatedDate = item.CreatedDate,
                    Color = item.Color,
                    Size = item.Size,
                    Price = item.Price,
                    StockCount = item.StockCount,
                    Condition = item.Condition,
                    MasterItemId = item.MasterItemId
                };

            if (!string.IsNullOrEmpty(request.Color))
            {
                predicate = predicate.And(item => EF.Functions.ILike(item.Color, $"%{request.Color}%"));
            }

            if (request.MinPrice != null)
            {
                predicate = predicate.And(item => item.Price >= request.MinPrice);
            }

            if (request.MaxPrice != null)
            {
                predicate = predicate.And(item => item.Price <= request.MaxPrice);
            }

            if (request.Size != null)
            {
                predicate = predicate.And(item => request.Size.Contains(item.Size));
            }

            if (request.Condition != null)
            {
                predicate = predicate.And(item => item.Condition.Equals(request.Condition));
            }

            (List<ItemVariationListResponse> Items, int Page, int PageSize, int TotalCount) result =
                await _fashionitemRepository.GetFashionItemVariationProjections(request.PageNumber, request.PageSize,
                    predicate,
                    selector);

            return new PaginationResponse<ItemVariationListResponse>()
            {
                Items = result.Items,
                PageSize = result.PageSize,
                PageNumber = result.Page,
                TotalCount = result.TotalCount
            };
        }*/

        public async Task<PaginationResponse<IndividualItemListResponse>> GetIndividualItemPagination(Guid masterItemId,
            IndividualItemRequest request)
        {
            Expression<Func<IndividualFashionItem, bool>> predicate = x => x.MasterItemId == masterItemId;
            Expression<Func<IndividualFashionItem, IndividualItemListResponse>> selector = item => new
                IndividualItemListResponse()
                {
                    MasterItemId = item.MasterItemId,
                    CreatedDate = item.CreatedDate,
                    ItemId = item.ItemId,
                    Type = item.Type,
                    Status = item.Status,
                    ItemCode = item.ItemCode,
                    SellingPrice = item.SellingPrice!.Value,
                    Color = item.Color,
                    Condition = item.Condition,
                    Image = item.Images.FirstOrDefault() != null ? item.Images.First().Url : string.Empty,
                    Size = item.Size,
                };

            if (!string.IsNullOrEmpty(request.SearchItemCode))
            {
                predicate = predicate.And(item => EF.Functions.ILike(item.ItemCode, $"%{request.SearchItemCode}%"));
            }

            if (request.MinSellingPrice != null)
            {
                predicate = predicate.And(item => item.SellingPrice >= request.MinSellingPrice);
            }

            if (request.MaxSellingPrice != null)
            {
                predicate = predicate.And(item => item.SellingPrice <= request.MaxSellingPrice);
            }

            if (request.Status != null)
            {
                predicate = predicate.And(item => request.Status.Contains(item.Status));
            }

            if (request.Types != null)
            {
                predicate = predicate.And(item => request.Types.Contains(item.Type));
            }

            (List<IndividualItemListResponse> Items, int Page, int PageSize, int TotalCount) result =
                await _fashionitemRepository.GetIndividualItemProjections(request.PageNumber, request.PageSize,
                    predicate,
                    selector);
            await CheckItemsInOrder(result.Items, request.MemberId);

            return new PaginationResponse<IndividualItemListResponse>()
            {
                Items = result.Items,
                PageSize = result.PageSize,
                PageNumber = result.Page,
                TotalCount = result.TotalCount
            };
        }
    }
}