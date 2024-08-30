using BusinessObjects.Dtos.Commons;
using BusinessObjects.Dtos.ConsignSaleLineItems;
using Repositories.ConsignSaleLineItems;

namespace Services.ConsignLineItems;

public class ConsignLineItemService : IConsignLineItemService
{
    private readonly IConsignSaleLineItemRepository _consignSaleLineItemRepository;

    public ConsignLineItemService(IConsignSaleLineItemRepository consignSaleLineItemRepository)
    {
        _consignSaleLineItemRepository = consignSaleLineItemRepository;
    }

    public async Task<DotNext.Result<ConsignSaleLineItemDetailedResponse, ErrorCode>> GetConsignLineItemById(
        Guid consignLineItemId)
    {
        return null;
    }
}