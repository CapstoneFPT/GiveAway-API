using System.Linq.Expressions;
using AutoMapper;
using AutoMapper.Internal;
using BusinessObjects.Dtos.AuctionDeposits;
using BusinessObjects.Dtos.AuctionItems;
using BusinessObjects.Dtos.Commons;
using BusinessObjects.Dtos.FashionItems;
using BusinessObjects.Entities;
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
                ShopId = shopId,
                Type = FashionItemType.ItemBase,
                Status = FashionItemStatus.Unavailable,
                SellingPrice = request.SellingPrice,
            };

            var newItem = await _fashionitemRepository.AddFashionItem(newdata);
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
                    ShopId = item.ShopId,
                    Type = item.Type,
                    Status = item.Status,
                    Color = item.Variation.Color,
                    SellingPrice = item.SellingPrice ?? 0,
                    ShopAddress = item.Shop.Address,
                    Images = item.Images.Select(x => x.Url).ToList()
                };

            if (!string.IsNullOrEmpty(request.SearchTerm))
            {
                predicate = predicate.And(item => EF.Functions.ILike(item.Variation.MasterItem.Name, $"%{request.SearchTerm}%"));
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
                predicate = predicate.And(item => item.ShopId == request.ShopId);
            }

            if (request.GenderType.HasValue)
            {
                predicate = predicate.And(item => item.Variation.MasterItem.Gender == request.GenderType);
            }

            (List<FashionItemDetailResponse> Items, int Page, int PageSize, int TotalCount) result =
                await _fashionitemRepository.GetFashionItemProjections(request.PageNumber, request.PageSize, predicate,
                    selector);
            
            await CheckItemsInOrder(result.Items, request.MemberId);
            
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

        private async Task CheckItemsInOrder(List<FashionItemDetailResponse> items, Guid? memberId)
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
            var item = await _fashionitemRepository.GetFashionItemById(id);
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
            var item = await _fashionitemRepository.GetFashionItemById(itemId);
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
            var item = await _fashionitemRepository.GetFashionItemById(itemId);
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
            var item = await _fashionitemRepository.GetFashionItemById(itemId);

            item.Status = request.Status;

            await _fashionitemRepository.UpdateFashionItem(item);
            return new Result<FashionItemDetailResponse?>
            {
                Data = _mapper.Map<FashionItemDetailResponse>(item),
                ResultStatus = ResultStatus.Success,
                Messages = ["Update Successfully"]
            };
        }
    }
}