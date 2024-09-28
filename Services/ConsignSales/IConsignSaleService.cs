using BusinessObjects.Dtos.Commons;
using BusinessObjects.Dtos.ConsignSaleLineItems;
using BusinessObjects.Dtos.ConsignSales;
using BusinessObjects.Dtos.FashionItems;
using BusinessObjects.Dtos.Orders;
using BusinessObjects.Entities;
using DotNext;
using Microsoft.AspNetCore.Mvc;

namespace Services.ConsignSales
{
    public interface IConsignSaleService
    {
        Task<Result<PaginationResponse<ConsignSaleListResponse>, ErrorCode>> GetAllConsignSales(Guid accountId,
            ConsignSaleRequest request);
        Task<DotNext.Result<ConsignSaleDetailedResponse, ErrorCode>> GetConsignSaleById(Guid consignId);
        Task<BusinessObjects.Dtos.Commons.Result<ConsignSaleDetailedResponse>> CreateConsignSale(Guid accountId, CreateConsignSaleRequest request);
        Task<BusinessObjects.Dtos.Commons.Result<ConsignSaleDetailedResponse>> ApprovalConsignSale(Guid consignId, ApproveConsignSaleRequest request);
        Task<BusinessObjects.Dtos.Commons.Result<ConsignSaleDetailedResponse>> ConfirmReceivedFromShop(Guid consignId);
        Task<BusinessObjects.Dtos.Commons.Result<ConsignSaleDetailedResponse>> CreateConsignSaleByShop(Guid shopId, CreateConsignSaleByShopRequest request);

        Task<Result<List<ConsignSaleLineItemsListResponse>, ErrorCode>> GetConsignSaleLineItems(Guid consignsaleId);
        Task<BusinessObjects.Dtos.Commons.Result<MasterItemResponse>> CreateMasterItemFromConsignSaleLineItem(Guid consignLineItemId, CreateMasterItemForConsignRequest detailRequest);

        /*Task<BusinessObjects.Dtos.Commons.Result<ItemVariationListResponse>> CreateVariationFromConsignSaleLineItem(Guid masteritemId,
            CreateItemVariationRequestForConsign request);*/

        Task<BusinessObjects.Dtos.Commons.Result<ConsignSaleLineItemResponse>> ConfirmConsignSaleLineReadyToSale(Guid consignLineItemId, ConfirmConsignSaleLineReadyToSaleRequest request);
        Task UpdateConsignPrice(Guid orderId);

        Task<Result<PaginationResponse<ConsignSaleListResponse>, ErrorCode>> GetConsignSales(
            ConsignSaleListRequest request);

        Task<BusinessObjects.Dtos.Commons.Result<ConsignSaleLineItemsListResponse>> ConfirmConsignSaleLineItemPrice(Guid consignLineItemId, decimal price);
        Task<BusinessObjects.Dtos.Commons.Result<ConsignSaleLineItemResponse>> NegotiateConsignSaleLineItem(Guid consignLineItemId, NegotiateConsignSaleLineRequest request);
        Task<BusinessObjects.Dtos.Commons.Result<ConsignSaleLineItemResponse>> ApproveNegotiation(Guid consignLineItemId);
        Task<BusinessObjects.Dtos.Commons.Result<ConsignSaleLineItemResponse>> RejectNegotiation(Guid consignLineItemId);
        Task<BusinessObjects.Dtos.Commons.Result<ConsignSaleLineItemResponse>> CreateIndividualAfterNegotiation(Guid consignLineItemId, CreateIndividualAfterNegotiationRequest request);
        Task<BusinessObjects.Dtos.Commons.Result<ConsignSaleDetailedResponse>> PostConsignSaleForSelling(Guid consignSaleId);
        Task<Result<ConsignSaleDetailedResponse, ErrorCode>> NotifyDelivery(Guid consignsaleId);
        Task<Result<ConsignSaleDetailedResponse, ErrorCode>> CancelAllConsignSaleLineItems(Guid consignsaleId);
        Task<BusinessObjects.Dtos.Commons.Result<ConsignSaleDetailedResponse>> NegotiatingConsignSale(Guid consignSaleId);
        Task<DotNext.Result<ExcelResponse, ErrorCode>> ExportConsignSaleToExcel(ExportConsignSaleToExcelRequest request);
        Task<Result<ConsignSaleDetailedResponse, ErrorCode>> ContinueConsignSale(Guid consignsaleId);

        Task<Result<InvoiceConsignResponse, ErrorCode>> GenerateConsignOfflineInvoice(Guid consignsaleId, Guid shopId);
    }
}
