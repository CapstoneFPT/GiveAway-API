using BusinessObjects.Dtos.AuctionDeposits;
using BusinessObjects.Dtos.AuctionItems;
using BusinessObjects.Dtos.Commons;
using BusinessObjects.Dtos.FashionItems;
using BusinessObjects.Entities;

namespace Services.FashionItems
{
    public interface IFashionItemService
    {
        Task<PaginationResponse<FashionItemList>> GetAllFashionItemPagination(FashionItemListRequest request);
        Task<Result<FashionItemDetailResponse>> GetFashionItemById(Guid id);
        Task<Result<FashionItemDetailResponse>> AddFashionItem(Guid shopId, FashionItemDetailRequest request);
        Task<Result<FashionItemDetailResponse>> UpdateFashionItem(Guid itemId, UpdateFashionItemRequest request);
        Task<Result<PaginationResponse<FashionItemDetailResponse>>> GetItemByCategoryHierarchy(Guid categoryId, AuctionFashionItemRequest request);
        Task<Result<FashionItemDetailResponse>> CheckFashionItemAvailability(Guid itemId);
        Task<List<IndividualFashionItem>> GetRefundableItems();
        Task ChangeToSoldItems(List<IndividualFashionItem> refundableItems);
        Task<Result<FashionItemDetailResponse?>> UpdateFashionItemStatus(Guid itemId, UpdateFashionItemStatusRequest request);
        Task<Result<List<MasterItemResponse>>> CreateMasterItemByAdmin(CreateMasterItemRequest masterItemRequest);
        Task<Result<ItemVariationResponse>> CreateItemVariation(Guid masteritemId,CreateItemVariationRequest variationRequest);

        Task<Result<List<IndividualItemListResponse>>> CreateIndividualItems(Guid variationId,
            List<CreateIndividualItemRequest> requests);

        Task<PaginationResponse<MasterItemListResponse>> GetAllMasterItemPagination(MasterItemRequest request);
        Task<PaginationResponse<ItemVariationListResponse>> GetAllFashionItemVariationPagination(Guid masterItemId,
            ItemVariationRequest request);
        Task<PaginationResponse<IndividualItemListResponse>> GetIndividualItemPagination(Guid variationId,
            IndividualItemRequest request);

        Task<DotNext.Result<MasterItemDetailResponse, ErrorCode>> GetMasterItemById(Guid id);

        Task<PaginationResponse<MasterItemListResponse>> GetMasterItemFrontPage(
            FrontPageMasterItemRequest request);
    }
}
