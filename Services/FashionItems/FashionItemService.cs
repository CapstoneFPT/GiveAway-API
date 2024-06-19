using AutoMapper;
using BusinessObjects.Dtos.AuctionDeposits;
using BusinessObjects.Dtos.AuctionItems;
using BusinessObjects.Dtos.Commons;
using BusinessObjects.Dtos.FashionItems;
using BusinessObjects.Entities;
using Repositories.FashionItems;

namespace Services.FashionItems
{
    public class FashionItemService : IFashionItemService
    {
        private readonly IFashionItemRepository _fashionitemRepository;
        private readonly IMapper _mapper;

        public FashionItemService(IFashionItemRepository fashionitemRepository, IMapper mapper)
        {
            _fashionitemRepository = fashionitemRepository;
            _mapper = mapper;
        }

        public async Task<Result<FashionItemDetailResponse>> AddFashionItem(Guid shopId, FashionItemDetailRequest request)
        {
            try
            {
                var response = new Result<FashionItemDetailResponse>();
                var item = new FashionItem();
                var newdata = _mapper.Map(request, item);
                newdata.ShopId = shopId;
                newdata.Status = FashionItemStatus.Available.ToString();    
                response.Data = _mapper.Map<FashionItemDetailResponse>(await _fashionitemRepository.AddFashionItem(newdata));
                response.Messages = ["Add successfully"];
                response.ResultStatus = ResultStatus.Success;
                return response;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<Result<PaginationResponse<FashionItemDetailResponse>>> GetAllFashionItemPagination(AuctionFashionItemRequest request)
        {
            try
            {
                var response = new Result<PaginationResponse<FashionItemDetailResponse>>();
                var result = await _fashionitemRepository.GetAllFashionItemPagination(request);
                if(result.TotalCount < 1)
                {
                    response.ResultStatus = ResultStatus.Empty;
                    response.Messages = ["Empty"];
                    return response;
                }
                response.Data = result;
                response.ResultStatus = ResultStatus.Success;
                response.Messages = ["Rsult in page: " + result.PageNumber];
                return response;
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        public async Task<Result<FashionItemDetailResponse>> GetFashionItemById(Guid id)
        {
            var response = new Result<FashionItemDetailResponse>();
            var item = await _fashionitemRepository.GetFashionItemById(id);
            if(item == null)
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

        public async Task<Result<FashionItemDetailResponse>> UpdateFashionItem(Guid itemId, Guid shopId, FashionItemDetailRequest request)
        {
            try
            {
                var response = new Result<FashionItemDetailResponse>();
                var item = await _fashionitemRepository.GetFashionItemById(itemId);
                if (!item.ShopId.Equals(shopId))
                {
                    response.Messages = ["Not allowed"];
                    response.ResultStatus = ResultStatus.Error;
                    return response;
                }
                var newdata = _mapper.Map(request, item);
                response.Data = _mapper.Map<FashionItemDetailResponse>(await _fashionitemRepository.UpdateFashionItem(newdata));
                response.ResultStatus = ResultStatus.Success;
                response.Messages = ["Update Successfully"];
                return response;
            }catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }
        public async Task<Result<PaginationResponse<FashionItemDetailResponse>>> GetItemByCategoryHierarchy(Guid categoryId, AuctionFashionItemRequest request)
        {
            try
            {
                var response = new Result<PaginationResponse<FashionItemDetailResponse>>();
                var items = await _fashionitemRepository.GetItemByCategoryHierarchy(categoryId, request);
                if(items.TotalCount > 0)
                {
                    response.Data = items;
                    response.ResultStatus = ResultStatus.Success;
                    response.Messages = ["Successfully with " + response.Data.TotalCount + " items"];
                    return response;
                }
                response.ResultStatus = ResultStatus.Empty;
                response.Messages = ["Empty"];
                return response;
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }
    }
}
