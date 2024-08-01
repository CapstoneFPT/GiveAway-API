using System.Linq.Expressions;
using AutoMapper;
using AutoMapper.Internal;
using BusinessObjects.Dtos.AuctionDeposits;
using BusinessObjects.Dtos.AuctionItems;
using BusinessObjects.Dtos.Commons;
using BusinessObjects.Dtos.FashionItems;
using BusinessObjects.Entities;
using Repositories.Categories;
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

        public FashionItemService(IFashionItemRepository fashionitemRepository, IImageRepository imageRepository,
            ICategoryRepository categoryRepository,
            IMapper mapper)
        {
            _fashionitemRepository = fashionitemRepository;
            _imageRepository = imageRepository;
            _categoryRepository = categoryRepository;
            _mapper = mapper;
        }

        public async Task<Result<FashionItemDetailResponse>> AddFashionItem(Guid shopId,
            FashionItemDetailRequest request)
        {
            var response = new Result<FashionItemDetailResponse>();
            var item = new FashionItem();
            var newdata = new FashionItem()
            {
                Name = request.Name,
                Note = !string.IsNullOrEmpty(request.Note) ? request.Note : null,
                Description = request.Description,
                Condition = request.Condition,
                Brand = request.Brand,
                Gender = request.Gender,
                Size = request.Size,
                CategoryId = request.CategoryId,
                ShopId = shopId,
                Type = FashionItemType.ItemBase,
                Status = FashionItemStatus.Unavailable,
                Color = request.Color,
                SellingPrice = request.SellingPrice,
                CreatedDate = DateTime.UtcNow,
                Description = request.Description
            };
            
            var newItem = await _fashionitemRepository.AddFashionItem(newdata);
            foreach (string img in request.Images)
            {
                var newimage = new Image()
                {
                    Url = img,
                    FashionItemId = newItem.ItemId,
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
            var response = new Result<PaginationResponse<FashionItemDetailResponse>>();
            var result = await _fashionitemRepository.GetAllFashionItemPagination(request);
            if (result.TotalCount < 1)
            {
                response.ResultStatus = ResultStatus.Success;
                response.Messages = ["Empty"];
                return response;
            }
            
            response.Data = result;
            response.ResultStatus = ResultStatus.Success;
            response.Messages = ["Results in page: " + result.PageNumber];
            return response;
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
            item.Name = request.Name ?? item.Name;
            item.Note = request.Note ?? item.Note;
            /*item.Value = request.Value.HasValue ? request.Value.Value : item.Value;*/
            item.Condition = request.Condition.HasValue ? request.Condition.Value : item.Condition;
            item.Brand = request.Brand ?? item.Brand;
            item.Color = request.Color ?? item.Color;
            item.Gender = request.Gender ?? item.Gender;    
            item.Size = request.Size ?? item.Size;
            item.CategoryId = request.CategoryId ?? item.CategoryId;
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

        public async Task<List<FashionItem>> GetRefundableItems()
        {
            Expression<Func<FashionItem, bool>> predicate = x => x.Status == FashionItemStatus.Refundable;
            var result = await _fashionitemRepository
                .GetFashionItems(predicate);
            return result;
        }

        public Task ChangeToSoldItems(List<FashionItem> refundableItems)
        {
            foreach (var item in refundableItems)
            {
                item.Status = FashionItemStatus.Sold;
            }
            return _fashionitemRepository.UpdateFashionItems(refundableItems);
        }

        public async Task<Result<FashionItemDetailResponse?>> UpdateFashionItemStatus(Guid itemId, UpdateFashionItemStatusRequest request)
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