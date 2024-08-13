using System.Linq.Expressions;
using AutoMapper;
using AutoMapper.Internal;
using BusinessObjects.Dtos.AuctionDeposits;
using BusinessObjects.Dtos.AuctionItems;
using BusinessObjects.Dtos.Commons;
using BusinessObjects.Dtos.FashionItems;
using BusinessObjects.Entities;
using DotNext.Collections.Generic;
using LinqKit;
using Microsoft.EntityFrameworkCore;
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

        public async Task<Result<FashionItemDetailResponse>> AddFashionItem(Guid shopId,
            FashionItemDetailRequest request)
        {
            var response = new Result<FashionItemDetailResponse>();
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

        public async Task<Result<PaginationResponse<FashionItemDetailResponse>>> GetAllFashionItemPagination(
            AuctionFashionItemRequest request)
        {
            Expression<Func<IndividualFashionItem, bool>> predicate = x => true;
            Expression<Func<IndividualFashionItem, FashionItemDetailResponse>> selector = item => new
                FashionItemDetailResponse()
                {
                    ItemId = item.ItemId,
                    Name = item.Variation.MasterItem.Name,
                    Note = item.Note,
                    Description = item.Variation.MasterItem.Description ?? string.Empty,
                    Condition = item.Variation.Condition,
                    Brand = item.Variation.MasterItem.Brand,
                    Gender = item.Variation.MasterItem.Gender,
                    Size = item.Variation.Size,
                    CategoryId = item.Variation.MasterItem.CategoryId,
                    CategoryName = item.Variation.MasterItem.Category.Name,
                    ShopId = item.Variation.MasterItem.ShopId,
                    Type = item.Type,
                    Status = item.Status,
                    Color = item.Variation.Color,
                    SellingPrice = item.SellingPrice ?? 0,
                    ShopAddress = item.Variation.MasterItem.Shop.Address,
                    Images = item.Images.Select(x => x.Url).ToList()
                };

            if (!string.IsNullOrEmpty(request.SearchTerm))
            {
                predicate = predicate.And(item =>
                    EF.Functions.ILike(item.Variation.MasterItem.Name, $"%{request.SearchTerm}%"));
            }

            if (request.Status != null)
            {
                predicate = predicate.And(item => request.Status.Contains(item.Status));
            }

            if (request.Type != null)
            {
                predicate = predicate.And(item => request.Type.Contains(item.Type));
            }

            if (request.CategoryId.HasValue)
            {
                var categoryIds = await _categoryRepository.GetAllChildrenCategoryIds(request.CategoryId.Value);
                predicate = predicate.And(item => categoryIds.Contains(item.Variation.MasterItem.CategoryId));
            }

            if (request.ShopId.HasValue)
            {
                predicate = predicate.And(item => item.Variation.MasterItem.ShopId == request.ShopId);
            }

            if (request.GenderType.HasValue)
            {
                predicate = predicate.And(item => item.Variation.MasterItem.Gender == request.GenderType);
            }

            (List<FashionItemDetailResponse> Items, int Page, int PageSize, int TotalCount) result =
                await _fashionitemRepository.GetIndividualItemProjections(request.PageNumber, request.PageSize,
                    predicate,
                    selector);

            // await CheckItemsInOrder(result.Items, request.MemberId);

            return new Result<PaginationResponse<FashionItemDetailResponse>>()
            {
                Data = new PaginationResponse<FashionItemDetailResponse>()
                {
                    Items = result.Items,
                    PageSize = result.PageSize,
                    PageNumber = result.Page,
                    TotalCount = result.TotalCount
                },
                ResultStatus = ResultStatus.Success
            };
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

        public async Task<Result<FashionItemDetailResponse>> GetFashionItemById(Guid id)
        {
            var response = new Result<FashionItemDetailResponse>();
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

        public async Task<Result<FashionItemDetailResponse>> UpdateFashionItem(Guid itemId,
            UpdateFashionItemRequest request)
        {
            var response = new Result<FashionItemDetailResponse>();
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

        public async Task<Result<PaginationResponse<FashionItemDetailResponse>>> GetItemByCategoryHierarchy(
            Guid categoryId, AuctionFashionItemRequest request)
        {
            var response = new Result<PaginationResponse<FashionItemDetailResponse>>();
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

        public async Task<Result<FashionItemDetailResponse>> CheckFashionItemAvailability(Guid itemId)
        {
            var response = new Result<FashionItemDetailResponse>();
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

        public async Task<Result<FashionItemDetailResponse?>> UpdateFashionItemStatus(Guid itemId,
            UpdateFashionItemStatusRequest request)
        {
            var item = await _fashionitemRepository.GetFashionItemById(c => c.ItemId == itemId);

            item.Status = request.Status;

            await _fashionitemRepository.UpdateFashionItem(item);
            return new Result<FashionItemDetailResponse?>
            {
                Data = _mapper.Map<FashionItemDetailResponse>(item),
                ResultStatus = ResultStatus.Success,
                Messages = ["Update Successfully"]
            };
        }

        public async Task<Result<List<MasterItemResponse>>> CreateMasterItemByAdmin(
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
                    MasterItemCode = await _fashionitemRepository.GenerateMasterItemCode(masterItemRequest.ItemCode),
                    CategoryId = masterItemRequest.CategoryId,
                    IsConsignment = true,
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

            return new Result<List<MasterItemResponse>>()
            {
                Data = _mapper.Map<List<MasterItemResponse>>(listMasterItemResponse),
                ResultStatus = ResultStatus.Success,
                Messages = new[] { "Add successfully" }
            };
        }

        public async Task<Result<ItemVariationResponse>> CreateItemVariation(Guid masteritemId,
            CreateItemVariationRequest variationRequest)
        {
            var itemVariation = new FashionItemVariation()
            {
                Color = variationRequest.Color,
                Condition = variationRequest.Condition,
                CreatedDate = DateTime.UtcNow,
                Size = variationRequest.Size,
                StockCount = variationRequest.IndividualItems.Length,
                MasterItemId = masteritemId,
                Price = variationRequest.Price
            };
            var itemVariationResponse = await _fashionitemRepository.AddSingleFashionItemVariation(itemVariation);
            var individualItemsResponse = new List<IndividualFashionItem>();
            foreach (var individualItem in variationRequest.IndividualItems)
            {
                var dataIndividualItem = new IndividualFashionItem()
                {
                    ItemCode = await _fashionitemRepository.GenerateIndividualItemCode(itemVariation.MasterItemId),
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
            return new Result<ItemVariationResponse>()
            {
                Data = _mapper.Map<ItemVariationResponse>(itemVariationResponse),
                ResultStatus = ResultStatus.Success,
                Messages = new[] { "Add new variation successfully" }
            };
        }

        public async Task<Result<List<IndividualItemListResponse>>> CreateIndividualItems(Guid variationId,
            List<CreateIndividualItemRequest> requests)
        {
            var individualItems = new List<IndividualFashionItem>();
            Expression<Func<MasterFashionItem, bool>> predicate = masterItem =>
                masterItem.Variations.Select(c => c.VariationId).Contains(variationId);
            var masterItem = await _fashionitemRepository.GetSingleMasterItem(predicate);
            Expression<Func<FashionItemVariation, bool>> expression = variation =>
                variation.VariationId == variationId;
            var fashionItemVariation = await _fashionitemRepository.GetSingleFashionItemVariation(expression);
            foreach (var individual in requests)
            {
                var dataIndividual = new IndividualFashionItem()
                {
                    Note = individual.Note,
                    VariationId = variationId,
                    CreatedDate = DateTime.UtcNow,
                    Status = FashionItemStatus.Unavailable,
                    Type = FashionItemType.ItemBase,
                    SellingPrice = individual.SellingPrice,
                    ItemCode = await _fashionitemRepository.GenerateIndividualItemCode(masterItem!.MasterItemId),
                };
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

            return new Result<List<IndividualItemListResponse>>()
            {
                Data = _mapper.Map<List<IndividualItemListResponse>>(individualItems),
                ResultStatus = ResultStatus.Success,
                Messages = new[] { "Add items successfully" }
            };
        }

        public async Task<Result<PaginationResponse<MasterItemListResponse>>> GetAllMasterItemPagination(
            MasterItemRequest request)
        {
            Expression<Func<MasterFashionItem, bool>> predicate = x => true;
            Expression<Func<MasterFashionItem, MasterItemListResponse>> selector = item => new
                MasterItemListResponse()
                {
                    MasterItemId = item.MasterItemId,
                    Name = item.Name,
                    Description = item.Description,
                    ItemCode = item.MasterItemCode,
                    CreatedDate = item.CreatedDate,
                    Brand = item.Brand,
                    Gender = item.Gender,
                    CategoryId = item.CategoryId,
                    IsUniversal = item.IsConsignment,
                    ShopId = item.ShopId,
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

            return new Result<PaginationResponse<MasterItemListResponse>>()
            {
                Data = new PaginationResponse<MasterItemListResponse>()
                {
                    Items = result.Items,
                    PageSize = result.PageSize,
                    PageNumber = result.Page,
                    TotalCount = result.TotalCount
                },
                ResultStatus = ResultStatus.Success,
                Messages = new[] { "Result with " + result.TotalCount + " items" }
            };
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

    public class MasterItemListResponse
    {
        public Guid MasterItemId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string ItemCode { get; set; }
        public DateTime CreatedDate { get; set; }
        public string Brand { get; set; }
        public GenderType Gender { get; set; }
        public Guid CategoryId { get; set; }
        public bool IsUniversal { get; set; }
        public Guid ShopId { get; set; }
        public List<string> Images { get; set; } = [];
    }
}