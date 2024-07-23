using BusinessObjects.Dtos.ConsignSaleDetails;

namespace Repositories.ConsignSaleDetails;

public interface IConsignSaleDetailRepository
{
    Task<List<ConsignSaleDetailResponse>> GetConsignSaleDetailsByConsignSaleId(Guid consignSaleId);
}