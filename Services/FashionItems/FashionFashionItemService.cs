using AutoMapper;
using BusinessObjects.Dtos.AuctionDeposits;
using BusinessObjects.Dtos.AuctionItems;
using BusinessObjects.Dtos.Commons;
using BusinessObjects.Dtos.FashionItems;
using Repositories.FashionItems;

namespace Services.FashionItems
{
    public class FashionFashionItemService : IFashionItemService
    {
        private readonly IFashionItemRepository _fashionitemRepository;
        private readonly IMapper _mapper;

        public FashionFashionItemService(IFashionItemRepository fashionitemRepository, IMapper mapper)
        {
            _fashionitemRepository = fashionitemRepository;
            _mapper = mapper;
        }
        public async Task<Result<PaginationResponse<FashionItemDetailResponse>>> GetAllFashionItemPagination(AuctionFashionItemRequest request)
        {
            try
            {
                var response = new Result<PaginationResponse<FashionItemDetailResponse>>();
                var result = await _fashionitemRepository.GetAllFashionItemPagination(request);
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
    }
}
