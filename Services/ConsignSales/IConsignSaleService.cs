using BusinessObjects.Dtos.Commons;
using BusinessObjects.Dtos.ConsignSaleDetails;
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
        Task<BusinessObjects.Dtos.Commons.Result<PaginationResponse<ConsignSaleResponse>>> GetAllConsignSalesByShopId(ConsignSaleRequestForShop request);
        
        Task<Result<List<ConsignSaleDetailResponse>, ErrorCode>> GetConsignSaleDetails(Guid consignsaleId);
        Task<BusinessObjects.Dtos.Commons.Result<MasterItemResponse>> CreateMasterItemFromConsignSaleDetail(Guid consignsaleId, CreateMasterItemForConsignRequest detailRequest);

        Task<BusinessObjects.Dtos.Commons.Result<ItemVariationListResponse>> CreateVariationFromConsignSaleDetail(Guid masteritemId,
            CreateItemVariationRequestForConsign request);

        Task<BusinessObjects.Dtos.Commons.Result<FashionItemDetailResponse>> CreateIndividualItemFromConsignSaleDetail(Guid consignsaledetailId,
            Guid variationId, CreateIndividualItemRequestForConsign request);
        Task UpdateConsignPrice(Guid orderId);
    }
}
