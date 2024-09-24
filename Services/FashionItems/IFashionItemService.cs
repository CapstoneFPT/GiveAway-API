using BusinessObjects.Dtos.AuctionDeposits;
using BusinessObjects.Dtos.AuctionItems;
using BusinessObjects.Dtos.Commons;
using BusinessObjects.Dtos.FashionItems;
using BusinessObjects.Dtos.Orders;
using BusinessObjects.Entities;
using DotNext;

namespace Services.FashionItems
{
    public interface IFashionItemService
    {
        Task<PaginationResponse<FashionItemList>> GetAllFashionItemPagination(FashionItemListRequest request);
        Task<BusinessObjects.Dtos.Commons.Result<FashionItemDetailResponse>> GetFashionItemById(Guid id, Guid? memberId);
        Task<BusinessObjects.Dtos.Commons.Result<FashionItemDetailResponse>> AddFashionItem(Guid shopId, FashionItemDetailRequest request);
        Task<BusinessObjects.Dtos.Commons.Result<FashionItemDetailResponse>> UpdateFashionItem(Guid itemId, UpdateFashionItemRequest request);
        Task<BusinessObjects.Dtos.Commons.Result<PaginationResponse<FashionItemDetailResponse>>> GetItemByCategoryHierarchy(Guid categoryId, AuctionFashionItemRequest request);
        Task<BusinessObjects.Dtos.Commons.Result<FashionItemDetailResponse>> CheckFashionItemAvailability(Guid itemId);
        Task<List<IndividualFashionItem>> GetRefundableItems();
        Task ChangeToSoldItems(List<IndividualFashionItem> refundableItems);
        Task<BusinessObjects.Dtos.Commons.Result<FashionItemDetailResponse?>> UpdateFashionItemStatus(Guid itemId, UpdateFashionItemStatusRequest request);
        Task<BusinessObjects.Dtos.Commons.Result<List<MasterItemResponse>>> CreateMasterItemByAdmin(CreateMasterItemRequest masterItemRequest);
        Task<BusinessObjects.Dtos.Commons.Result<MasterItemResponse>> UpdateMasterItem(Guid masteritemId, UpdateMasterItemRequest masterItemRequest);

        Task<BusinessObjects.Dtos.Commons.Result<List<IndividualItemListResponse>>> CreateIndividualItems(Guid masterItemId,
            CreateIndividualItemRequest requests);

        Task<PaginationResponse<MasterItemListResponse>> GetAllMasterItemPagination(MasterItemRequest request);

        Task<PaginationResponse<IndividualItemListResponse>> GetIndividualItemPagination(Guid masterItemId,
            IndividualItemRequest request);
        Task<Result<ExcelResponse, ErrorCode>> ExportFashionItemsToExcel(ExportFashionItemsRequest request);
        Task<DotNext.Result<MasterItemDetailResponse, ErrorCode>> GetMasterItemById(Guid id);

        Task<PaginationResponse<MasterItemListResponse>> GetMasterItemFrontPage(
            FrontPageMasterItemRequest request);

        Task<DotNext.Result<MasterItemDetailResponse, ErrorCode>> FindMasterItem(FindMasterItemRequest request);
        Task<BusinessObjects.Dtos.Commons.Result<string?>> DeleteDraftItem(List<DeleteDraftItemRequest> deleteDraftItemRequests);
        Task<Result<FashionItemDetailResponse, ErrorCode>> AddReturnedItemToShop(Guid itemId);
        Task<DotNext.Result<MasterItemResponse, ErrorCode>> CreateMasterItemForOfflineConsign(Guid shopId, CreateMasterOfflineConsignRequest request);
    }
}
