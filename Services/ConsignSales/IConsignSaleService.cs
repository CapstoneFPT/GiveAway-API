using BusinessObjects.Dtos.Commons;
using BusinessObjects.Dtos.ConsignSales;

namespace Services.ConsignSales
{
    public interface IConsignSaleService
    {
        Task<Result<PaginationResponse<ConsignSaleResponse>>> GetAllConsignSales(Guid accountId, ConsignSaleRequest request);
        Task<Result<ConsignSaleResponse>> GetConsignSaleById(Guid consignId);
        Task<Result<ConsignSaleResponse>> CreateConsignSale(Guid accountId, CreateConsignSaleRequest request);
    }
}
