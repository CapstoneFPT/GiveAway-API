﻿using System.Linq.Expressions;
using BusinessObjects.Dtos.Commons;
using BusinessObjects.Dtos.ConsignSaleLineItems;
using BusinessObjects.Entities;
using DotNext;
using Microsoft.EntityFrameworkCore;
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
                    ProductName = item.ProductName,
                    Condition = item.Condition,
                    Images = item.Images.Select(x => x.Url ?? string.Empty).ToList(),
                    ConfirmedPrice = item.ConfirmedPrice,
                    DealPrice = item.DealPrice,
                    CreatedDate = item.CreatedDate,
                    ConsignSaleCode = item.ConsignSale.ConsignSaleCode,
                    Brand = item.Brand,
                    Color = item.Color,
                    Size = item.Size,
                    Gender = item.Gender,
                    Note = item.Note,
                    FashionItemStatus = item.IndividualFashionItem.Status
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
            return new Result<ConsignSaleLineItemDetailedResponse, ErrorCode>(ErrorCode.ServerError);
        }
    }
}