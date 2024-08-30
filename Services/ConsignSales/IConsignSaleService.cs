using BusinessObjects.Dtos.Commons;
using BusinessObjects.Dtos.ConsignSaleLineItems;
using BusinessObjects.Dtos.ConsignSales;
using BusinessObjects.Dtos.FashionItems;
using BusinessObjects.Entities;
using DotNext;
using Microsoft.AspNetCore.Mvc;

namespace Services.ConsignSales
{
    public interface IConsignSaleService
    {
        Task<BusinessObjects.Dtos.Commons.Result<PaginationResponse<ConsignSaleDetailedResponse>>> GetAllConsignSales(Guid accountId, ConsignSaleRequest request);
        Task<DotNext.Result<ConsignSaleDetailedResponse,ErrorCode>> GetConsignSaleById(Guid consignId);
        Task<BusinessObjects.Dtos.Commons.Result<ConsignSaleDetailedResponse>> CreateConsignSale(Guid accountId, CreateConsignSaleRequest request);
        Task<BusinessObjects.Dtos.Commons.Result<ConsignSaleDetailedResponse>> ApprovalConsignSale(Guid consignId, ApproveConsignSaleRequest request);
        Task<BusinessObjects.Dtos.Commons.Result<ConsignSaleDetailedResponse>> ConfirmReceivedFromShop(Guid consignId);
        Task<BusinessObjects.Dtos.Commons.Result<ConsignSaleDetailedResponse>> CreateConsignSaleByShop(Guid shopId, CreateConsignSaleByShopRequest request);
        
        Task<Result<List<ConsignSaleLineItemsListResponse>, ErrorCode>> GetConsignSaleLineItems(Guid consignsaleId);
        Task<BusinessObjects.Dtos.Commons.Result<MasterItemResponse>> CreateMasterItemFromConsignSaleLineItem(Guid consignsaleId, CreateMasterItemForConsignRequest detailRequest);

        /*Task<BusinessObjects.Dtos.Commons.Result<ItemVariationListResponse>> CreateVariationFromConsignSaleLineItem(Guid masteritemId,
            CreateItemVariationRequestForConsign request);*/

        Task<BusinessObjects.Dtos.Commons.Result<FashionItemDetailResponse>> CreateIndividualItemFromConsignSaleLineItem(Guid consignsaledetailId,
            Guid masterItemId, CreateIndividualItemRequestForConsign request);
        Task UpdateConsignPrice(Guid orderId);

        Task<Result<PaginationResponse<ConsignSaleListResponse>, ErrorCode>> GetConsignSales(
            ConsignSaleListRequest request);
    }
}
