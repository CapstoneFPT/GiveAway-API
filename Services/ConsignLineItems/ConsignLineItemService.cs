using System.Linq.Expressions;
using BusinessObjects.Dtos.Commons;
using BusinessObjects.Dtos.ConsignSaleLineItems;
using BusinessObjects.Entities;
using DotNext;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Repositories.ConsignSaleLineItems;

namespace Services.ConsignLineItems;

public class ConsignLineItemService : IConsignLineItemService
{
    private readonly IConsignSaleLineItemRepository _consignSaleLineItemRepository;
    private readonly ILogger<ConsignLineItemService> _logger;

    public ConsignLineItemService(IConsignSaleLineItemRepository consignSaleLineItemRepository,
        ILogger<ConsignLineItemService> logger)
    {
        _logger = logger;
        _consignSaleLineItemRepository = consignSaleLineItemRepository;
    }

    public async Task<DotNext.Result<ConsignSaleLineItemDetailedResponse, ErrorCode>> GetConsignLineItemById(
        Guid consignLineItemId)
    {
        try
        {
            var query = _consignSaleLineItemRepository.GetQueryable();

            Expression<Func<ConsignSaleLineItem, bool>> predicate = lineItem =>
                lineItem.ConsignSaleLineItemId == consignLineItemId;
            Expression<Func<ConsignSaleLineItem, ConsignSaleLineItemDetailedResponse>> selector = item =>
                new ConsignSaleLineItemDetailedResponse()
                {
                    ConsignSaleLineItemId = item.ConsignSaleLineItemId,
                    ConsignSaleId = item.ConsignSaleId,
                    Status = item.Status,
                    ProductName = item.ProductName,
                    Condition = item.Condition,
                    Images = item.Images.Select(x => x.Url ?? string.Empty).ToList(),
                    ConfirmedPrice = item.ConfirmedPrice,
                    DealPrice = item.DealPrice ?? 0,
                    ExpectedPrice = item.ExpectedPrice,
                    CreatedDate = item.CreatedDate,
                    ConsignSaleCode = item.ConsignSale.ConsignSaleCode,
                    Brand = item.Brand,
                    Color = item.Color,
                    Size = item.Size,
                    Gender = item.Gender,
                    Note = item.Note,
                    IsApproved = item.IsApproved,
                    ShopResponse = item.ResponseFromShop,
                    FashionItemStatus = item.IndividualFashionItem.Status,
                    IndividualItemId = item.IndividualFashionItem.ItemId,
                    ItemCode = item.IndividualFashionItem.ItemCode
                };

            var result = await query
                .Include(x => x.ConsignSale)
                .Include(x => x.IndividualFashionItem)
                .Where(predicate)
                .Select(selector)
                .FirstOrDefaultAsync();

            if (result == null)
            {
                return new Result<ConsignSaleLineItemDetailedResponse, ErrorCode>(ErrorCode.NotFound);
            }

            return new Result<ConsignSaleLineItemDetailedResponse, ErrorCode>(result);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error fetching consign sale line item details");
            return new Result<ConsignSaleLineItemDetailedResponse, ErrorCode>(ErrorCode.ServerError);
        }
    }
}