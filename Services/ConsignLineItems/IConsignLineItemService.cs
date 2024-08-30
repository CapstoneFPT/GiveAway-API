using BusinessObjects.Dtos.Commons;
using BusinessObjects.Dtos.ConsignSaleLineItems;
using DotNext;

namespace Services.ConsignLineItems;

public interface IConsignLineItemService
{
    Task<Result<ConsignSaleLineItemDetailedResponse, ErrorCode>> GetConsignLineItemById(Guid consignLineItemId);
}