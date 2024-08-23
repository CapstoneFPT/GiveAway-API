using System.Linq.Expressions;
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

namespace Services.FashionItems
{
    public class FashionItemService : IFashionItemService
    {
        private readonly IFashionItemRepository _fashionitemRepository;
        private readonly IImageRepository _imageRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IMapper _mapper;
        private readonly IConsignSaleRepository _consignSaleRepository;

        public FashionItemService(IFashionItemRepository fashionitemRepository, IImageRepository imageRepository,
            ICategoryRepository categoryRepository,
            IMapper mapper, IConsignSaleRepository consignSaleRepository)
        {
            _fashionitemRepository = fashionitemRepository;
            _imageRepository = imageRepository;
            _categoryRepository = categoryRepository;
            _mapper = mapper;
            _consignSaleRepository = consignSaleRepository;
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
                    Name = item.Variation != null ? item.Variation.MasterItem.Name : string.Empty,
                    Note = item.Note ?? string.Empty,
                    CategoryId = item.Variation != null ? item.Variation.MasterItem.CategoryId : Guid.Empty,
                    Condition = item.Variation != null ? item.Variation.Condition : string.Empty,
                    Brand = item.Variation != null ? item.Variation.MasterItem.Brand : string.Empty,
                    Gender = item.Variation != null ? item.Variation.MasterItem.Gender : 0,
                    Size = item.Variation != null ? item.Variation.Size : 0,
                    Type = item.Type,
                    Status = item.Status,
                    Color = item.Variation != null ? item.Variation.Color : string.Empty,
                    SellingPrice = item.SellingPrice ?? 0,
                    Image = item.Images.FirstOrDefault() != null ? item.Images.First().Url : string.Empty,
                    MasterItemId = item.Variation != null ? item.Variation.MasterItemId : Guid.Empty,
                    ShopId = item.Variation != null ? item.Variation.MasterItem.ShopId : Guid.Empty,
                    ItemCode = item.ItemCode,
                    VariationId = item.VariationId
                };

            if (!string.IsNullOrEmpty(request.Name))
            {
                predicate = predicate.And(item =>
                    item.Variation != null && EF.Functions.ILike(item.Variation.MasterItem.Name, $"%{request.Name}%"));
            }

            if (!string.IsNullOrEmpty(request.MasterItemCode))
            {
                predicate = predicate
                    .And(item =>
                        item.Variation != null &&
                        EF.Functions.ILike(item.Variation.MasterItem.MasterItemCode, $"%{request.MasterItemCode}%"));
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
                    item.Variation != null && categoryIds.Contains(item.Variation.MasterItem.CategoryId));
            }

            if (request.ShopId.HasValue)
            {
                predicate = predicate.And(item =>
                    item.Variation != null && item.Variation.MasterItem.ShopId == request.ShopId);
            }

            if (request.Gender.HasValue)
            {
                predicate = predicate.And(item =>
                    item.Variation != null && item.Variation.MasterItem.Gender == request.Gender);
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
                predicate = predicate.And(item => item.Variation != null && item.Variation.Color == request.Color);
            }

            if (request.Size != null)
            {
                predicate = predicate.And(item => item.Variation != null && item.Variation.Size == request.Size);
            }

            if (request.MasterItemId != null)
            {
                predicate = predicate.And(item =>
                    item.Variation != null && item.Variation.MasterItemId == request.MasterItemId);
            }

            if (!string.IsNullOrEmpty(request.Condition))
            {
                predicate = predicate.And(item => item.Variation != null && item.Variation.Condition == request.Condition);
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

        public async Task<BusinessObjects.Dtos.Commons.Result<FashionItemDetailResponse>> GetFashionItemById(Guid id)
        {
            var response = new BusinessObjects.Dtos.Commons.Result<FashionItemDetailResponse>();
            var item = await _fashionitemRepository.GetFashionItemById(c => c.ItemId == id);
            if (item == null)
            {
                response.Messages = ["Item is not existed"];
                response.ResultStatus = ResultStatus.NotFound;
                return response;
            }

            response.Data = _mapper.Map<FashionItemDetailResponse>(item);
            response.ResultStatus = ResultStatus.Success;
            response.Messages = ["Successfully"];
            return response;
        }

        public async Task<BusinessObjects.Dtos.Commons.Result<FashionItemDetailResponse>> UpdateFashionItem(Guid itemId,
            UpdateFashionItemRequest request)
        {
            var response = new BusinessObjects.Dtos.Commons.Result<FashionItemDetailResponse>();
            var item = await _fashionitemRepository.GetFashionItemById(c => c.ItemId == itemId);
            if (item is null)
            {
                response.Messages = ["Item is not found"];
                response.ResultStatus = ResultStatus.Error;
                return response;
            }

            ;
            item.SellingPrice = request.SellingPrice.HasValue ? request.SellingPrice.Value : item.SellingPrice;
            item.Note = request.Note ?? item.Note;
            /*item.Value = request.Value.HasValue ? request.Value.Value : item.Value;*/
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
            if (item != null)
            {
                if (item.Status.Equals(FashionItemStatus.Unavailable))
                {
                    item.Status = FashionItemStatus.Available;
                    await _fashionitemRepository.UpdateFashionItem(item);
                    // var consign =
                    //     await _consignSaleRepository.GetSingleConsignSale(c => c.ConsignSaleDetails.Any(c => c.FashionItemId.Equals(item.ItemId)));
                    /*if (consign != null)
                    {
                        if (!consign.ConsignSaleDetails.Any(c => c.FashionItem.Status.Equals(FashionItemStatus.Unavailable)))
                        {
                            consign.Status = ConsignSaleStatus.OnSale;
                            await _consignSaleRepository.UpdateConsignSale(consign);
                        }
                    }*/
                    response.Messages = ["This item status has successfully changed to available"];
                    response.Data = _mapper.Map<FashionItemDetailResponse>(item);
                    response.ResultStatus = ResultStatus.Success;
                    return response;
                }

                item.Status = FashionItemStatus.Unavailable;
                await _fashionitemRepository.UpdateFashionItem(item);
                response.Data = _mapper.Map<FashionItemDetailResponse>(item);
                response.Messages = ["This item status has successfully changed to unavailable"];
                response.ResultStatus = ResultStatus.Success;
                return response;
            }

            response.Messages = ["Can not found the item"];
            response.ResultStatus = ResultStatus.NotFound;
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
            foreach (var shopId in masterItemRequest.ShopId)
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
                    IsConsignment = false,
                    CreatedDate = DateTime.UtcNow
                };
                masterItem.ShopId = shopId;
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

            return new BusinessObjects.Dtos.Commons.Result<List<MasterItemResponse>>()
            {
                Data = _mapper.Map<List<MasterItemResponse>>(listMasterItemResponse),
                ResultStatus = ResultStatus.Success,
                Messages = new[] { "Add successfully" }
            };
        }

        public async Task<BusinessObjects.Dtos.Commons.Result<ItemVariationResponse>> CreateItemVariation(
            Guid masteritemId,
            CreateItemVariationRequest variationRequest)
        {
            if (variationRequest.IndividualItems.Length > variationRequest.StockCount)
            {
                throw new OverStockException("You have added item more than permitted quantity in stock");
            }

            var itemVariation = new FashionItemVariation()
            {
                Color = variationRequest.Color,
                Condition = variationRequest.Condition,
                CreatedDate = DateTime.UtcNow,
                Size = variationRequest.Size,
                StockCount = variationRequest.StockCount,
                MasterItemId = masteritemId,
                Price = variationRequest.Price
            };
            var itemVariationResponse = await _fashionitemRepository.AddSingleFashionItemVariation(itemVariation);
            var individualItemsResponse = new List<IndividualFashionItem>();
            var masterItem = await _fashionitemRepository.GetSingleMasterItem(c => c.MasterItemId == masteritemId);
            foreach (var individualItem in variationRequest.IndividualItems)
            {
                var dataIndividualItem = new IndividualFashionItem()
                {
                    ItemCode = await _fashionitemRepository.GenerateIndividualItemCode(masterItem!.MasterItemCode),
                    SellingPrice = individualItem.SellingPrice,
                    Note = individualItem.Note,
                    VariationId = itemVariation.VariationId,
                    Status = FashionItemStatus.Unavailable,
                    Type = FashionItemType.ItemBase,
                    CreatedDate = DateTime.UtcNow
                };
                dataIndividualItem = await _fashionitemRepository.AddInvidualFashionItem(dataIndividualItem);

                var individualItemImages = new List<Image>();
                foreach (var image in individualItem.Images)
                {
                    var dataImage = new Image()
                    {
                        Url = image,
                        CreatedDate = DateTime.UtcNow,
                        IndividualFashionItemId = dataIndividualItem.ItemId,
                    };
                    individualItemImages.Add(dataImage);
                }

                await _imageRepository.AddRangeImage(individualItemImages);
                dataIndividualItem.Images = individualItemImages;
                individualItemsResponse.Add(dataIndividualItem);
            }

            itemVariationResponse.IndividualItems = individualItemsResponse;
            return new BusinessObjects.Dtos.Commons.Result<ItemVariationResponse>()
            {
                Data = _mapper.Map<ItemVariationResponse>(itemVariationResponse),
                ResultStatus = ResultStatus.Success,
                Messages = new[] { "Add new variation successfully" }
            };
        }

        public async Task<BusinessObjects.Dtos.Commons.Result<List<IndividualItemListResponse>>> CreateIndividualItems(
            Guid variationId,
            List<CreateIndividualItemRequest> requests)
        {
            var individualItems = new List<IndividualFashionItem>();
            Expression<Func<MasterFashionItem, bool>> predicate = masterItem =>
                masterItem.Variations.Select(c => c.VariationId).Contains(variationId);
            var masterItem = await _fashionitemRepository.GetSingleMasterItem(predicate);
            Expression<Func<FashionItemVariation, bool>> expression = variation =>
                variation.VariationId == variationId;
            var fashionItemVariation = await _fashionitemRepository.GetSingleFashionItemVariation(expression);
            if ((fashionItemVariation.IndividualItems.Count + requests.Count) > fashionItemVariation.StockCount)
            {
                throw new OverStockException("You have added item more than permitted quantity in stock");
            }

            foreach (var individual in requests)
            {
                var dataIndividual = new IndividualFashionItem()
                {
                    Note = individual.Note,
                    VariationId = variationId,
                    CreatedDate = DateTime.UtcNow,
                    // Status = FashionItemStatus.Unavailable,
                    Type = FashionItemType.ItemBase,
                    SellingPrice = individual.SellingPrice,
                    ItemCode = await _fashionitemRepository.GenerateIndividualItemCode(masterItem!.MasterItemCode),
                };
                dataIndividual.Status = masterItem.IsConsignment == false ? FashionItemStatus.Unavailable : FashionItemStatus.PendingForConsignSale;
                dataIndividual = await _fashionitemRepository.AddInvidualFashionItem(dataIndividual);

                var individualItemImages = new List<Image>();
                foreach (var image in individual.Images)
                {
                    var dataItemImage = new Image()
                    {
                        Url = image,
                        CreatedDate = DateTime.UtcNow,
                        IndividualFashionItemId = dataIndividual.ItemId,
                    };
                    individualItemImages.Add(dataItemImage);
                }

                await _imageRepository.AddRangeImage(individualItemImages);
                dataIndividual.Images = individualItemImages;

                individualItems.Add(dataIndividual);
            }

            fashionItemVariation.StockCount += individualItems.Count;
            await _fashionitemRepository.UpdateFashionItemVariation(fashionItemVariation);

            return new BusinessObjects.Dtos.Commons.Result<List<IndividualItemListResponse>>()
            {
                Data = _mapper.Map<List<IndividualItemListResponse>>(individualItems),
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
                    ShopId = item.ShopId,
                    StockCount = item.Variations
                        .Sum(x => x.IndividualItems
                            .Count(fashionItem => fashionItem
                                .Status == FashionItemStatus.Available)),
                    Images = item.Images.Select(x => x.Url).ToList()
                };

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

            if (request.ShopId.HasValue)
            {
                predicate = predicate.And(item => item.ShopId == request.ShopId);
            }

            if (request.GenderType.HasValue)
            {
                predicate = predicate.And(item => item.Gender == request.GenderType);
            }

            (List<MasterItemListResponse> Items, int Page, int PageSize, int TotalCount) result =
                await _fashionitemRepository.GetMasterItemProjections(request.PageNumber, request.PageSize, predicate,
                    selector);

            return new PaginationResponse<MasterItemListResponse>()
            {
                Items = result.Items,
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
                    StockCount = g.Sum(m =>
                        m.Variations.Sum(v => v.IndividualItems.Count(i => i.Status == FashionItemStatus.Available))),
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
                    StockCount = item.Variations
                        .Sum(x => x.IndividualItems
                            .Count(fashionItem => fashionItem
                                .Status == FashionItemStatus.Available)),
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

        public async Task<PaginationResponse<ItemVariationListResponse>> GetAllFashionItemVariationPagination(
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
        }

        public async Task<PaginationResponse<IndividualItemListResponse>> GetIndividualItemPagination(Guid variationId,
            IndividualItemRequest request)
        {
            Expression<Func<IndividualFashionItem, bool>> predicate = x => x.VariationId == variationId;
            Expression<Func<IndividualFashionItem, IndividualItemListResponse>> selector = item => new
                IndividualItemListResponse()
                {
                    VariationId = item.VariationId,
                    CreatedDate = item.CreatedDate,
                    ItemId = item.ItemId,
                    Type = item.Type,
                    Status = item.Status,
                    ItemCode = item.ItemCode,
                    SellingPrice = item.SellingPrice!.Value,
                    Color = item.Variation != null ? item.Variation.Color : string.Empty,
                    Condition = item.Variation != null ? item.Variation.Condition : string.Empty,
                    Image = item.Images.FirstOrDefault() != null ? item.Images.First().Url : string.Empty,
                    Size = item.Variation != null ? item.Variation.Size : 0
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