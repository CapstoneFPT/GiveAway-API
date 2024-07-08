using BusinessObjects.Dtos.AuctionDeposits;
using BusinessObjects.Dtos.AuctionItems;
using BusinessObjects.Dtos.Commons;
using BusinessObjects.Dtos.FashionItems;

namespace Services.FashionItems
{
    public interface IFashionItemService
    {
        Task<Result<PaginationResponse<FashionItemDetailResponse>>> GetAllFashionItemPagination(AuctionFashionItemRequest request);
        Task<Result<FashionItemDetailResponse>> GetFashionItemById(Guid id);
        Task<Result<FashionItemDetailResponse>> AddFashionItem(Guid shopId, FashionItemDetailRequest request);
        Task<Result<FashionItemDetailResponse>> UpdateFashionItem(Guid itemId, FashionItemDetailRequest request);
        Task<Result<PaginationResponse<FashionItemDetailResponse>>> GetItemByCategoryHierarchy(Guid id, AuctionFashionItemRequest request);
        Task<Result<FashionItemDetailResponse>> CheckFashionItemAvailability(Guid itemId); 
    }
}
