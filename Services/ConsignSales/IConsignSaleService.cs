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
        Task<BusinessObjects.Dtos.Commons.Result<PaginationResponse<ConsignSaleResponse>>> GetAllConsignSales(Guid accountId, ConsignSaleRequest request);
        Task<BusinessObjects.Dtos.Commons.Result<ConsignSaleResponse>> GetConsignSaleById(Guid consignId);
        Task<BusinessObjects.Dtos.Commons.Result<ConsignSaleResponse>> CreateConsignSale(Guid accountId, CreateConsignSaleRequest request);
        Task<BusinessObjects.Dtos.Commons.Result<ConsignSaleResponse>> ApprovalConsignSale(Guid consignId, ApproveConsignSaleRequest request);
        Task<BusinessObjects.Dtos.Commons.Result<ConsignSaleResponse>> ConfirmReceivedFromShop(Guid consignId);
        Task<BusinessObjects.Dtos.Commons.Result<ConsignSaleResponse>> CreateConsignSaleByShop(Guid shopId, CreateConsignSaleByShopRequest request);
        
        Task<Result<List<ConsignSaleLineItemsListResponse>, ErrorCode>> GetConsignSaleLineItems(Guid consignsaleId);
        Task<BusinessObjects.Dtos.Commons.Result<MasterItemResponse>> CreateMasterItemFromConsignSaleLineItem(Guid consignsaleId, CreateMasterItemForConsignRequest detailRequest);

        Task<BusinessObjects.Dtos.Commons.Result<ItemVariationListResponse>> CreateVariationFromConsignSaleLineItem(Guid masteritemId,
            CreateItemVariationRequestForConsign request);

        Task<BusinessObjects.Dtos.Commons.Result<FashionItemDetailResponse>> CreateIndividualItemFromConsignSaleLineItem(Guid consignsaledetailId,
            Guid variationId, CreateIndividualItemRequestForConsign request);
        Task UpdateConsignPrice(Guid orderId);

        Task<Result<PaginationResponse<ConsignSaleListResponse>, ErrorCode>> GetConsignSales(
            ConsignSaleListRequest request);
    }
}
