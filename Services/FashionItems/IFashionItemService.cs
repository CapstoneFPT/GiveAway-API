using BusinessObjects.Dtos.AuctionDeposits;
using BusinessObjects.Dtos.AuctionItems;
using BusinessObjects.Dtos.Commons;
using BusinessObjects.Dtos.FashionItems;
using BusinessObjects.Entities;

namespace Services.FashionItems
{
    public interface IFashionItemService
    {
        Task<Result<PaginationResponse<FashionItemDetailResponse>>> GetAllFashionItemPagination(AuctionFashionItemRequest request);
        Task<Result<FashionItemDetailResponse>> GetFashionItemById(Guid id);
        Task<Result<FashionItemDetailResponse>> AddFashionItem(Guid shopId, FashionItemDetailRequest request);
        Task<Result<FashionItemDetailResponse>> UpdateFashionItem(Guid itemId, UpdateFashionItemRequest request);
        Task<Result<PaginationResponse<FashionItemDetailResponse>>> GetItemByCategoryHierarchy(Guid categoryId, AuctionFashionItemRequest request);
        Task<Result<FashionItemDetailResponse>> CheckFashionItemAvailability(Guid itemId);
        Task<List<IndividualFashionItem>> GetRefundableItems();
        Task ChangeToSoldItems(List<IndividualFashionItem> refundableItems);
        Task<Result<FashionItemDetailResponse?>> UpdateFashionItemStatus(Guid itemId, UpdateFashionItemStatusRequest request);
    }
}
