using BusinessObjects.Dtos.Commons;
using BusinessObjects.Dtos.ConsignSaleDetails;
using BusinessObjects.Dtos.ConsignSales;
using BusinessObjects.Dtos.FashionItems;
using BusinessObjects.Entities;
using Microsoft.AspNetCore.Mvc;

namespace Services.ConsignSales
{
    public interface IConsignSaleService
    {
        Task<Result<PaginationResponse<ConsignSaleResponse>>> GetAllConsignSales(Guid accountId, ConsignSaleRequest request);
        Task<Result<ConsignSaleResponse>> GetConsignSaleById(Guid consignId);
        Task<Result<ConsignSaleResponse>> CreateConsignSale(Guid accountId, CreateConsignSaleRequest request);
        Task<Result<ConsignSaleResponse>> ApprovalConsignSale(Guid consignId, ApproveConsignSaleRequest request);
        Task<Result<ConsignSaleResponse>> ConfirmReceivedFromShop(Guid consignId);
        Task<Result<ConsignSaleResponse>> CreateConsignSaleByShop(Guid shopId, CreateConsignSaleByShopRequest request);
        Task<Result<PaginationResponse<ConsignSaleResponse>>> GetAllConsignSalesByShopId(ConsignSaleRequestForShop request);
        
        Task<Result<List<ConsignSaleDetailResponse>>> GetConsignSaleDetailsByConsignSaleId(Guid consignsaleId);
        Task<Result<MasterItemResponse>> CreateMasterItemFromConsignSaleDetail(Guid consignsaleId, CreateMasterItemForConsignRequest detailRequest);

        Task<Result<ItemVariationListResponse>> CreateVariationFromConsignSaleDetail(Guid masteritemId,
            CreateItemVariationRequestForConsign request);

        Task<Result<FashionItemDetailResponse>> CreateIndividualItemFromConsignSaleDetail(Guid consignsaledetailId,
            Guid variationId, CreateIndividualItemRequestForConsign request);
        Task UpdateConsignPrice(Guid orderId);
    }
}
