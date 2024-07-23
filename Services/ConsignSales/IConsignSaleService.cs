using BusinessObjects.Dtos.Commons;
using BusinessObjects.Dtos.ConsignSaleDetails;
using BusinessObjects.Dtos.ConsignSales;
using BusinessObjects.Entities;
using Microsoft.AspNetCore.Mvc;

namespace Services.ConsignSales
{
    public interface IConsignSaleService
    {
        Task<Result<PaginationResponse<ConsignSaleResponse>>> GetAllConsignSales(Guid accountId, ConsignSaleRequest request);
        Task<Result<ConsignSaleResponse>> GetConsignSaleById(Guid consignId);
        Task<Result<ConsignSaleResponse>> CreateConsignSale(Guid accountId, CreateConsignSaleRequest request);
        Task<Result<ConsignSaleResponse>> ApprovalConsignSale(Guid consignId, ConsignSaleStatus status);
        Task<Result<ConsignSaleResponse>> ConfirmReceivedFromShop(Guid consignId);
        Task<Result<ConsignSaleResponse>> CreateConsignSaleByShop(Guid shopId, CreateConsignSaleByShopRequest request);
        Task<Result<PaginationResponse<ConsignSaleResponse>>> GetAllConsignSalesByShopId(Guid shopId, ConsignSaleRequestForShop request);
        Task<Result<string>> SendEmailConsignSale(Guid consignSaleId);
        Task<Result<List<ConsignSaleDetailResponse>>> GetConsignSaleDetailsByConsignSaleId(Guid consignsaleId);
    }
}
