using System.Data.Entity;
using System.Linq.Expressions;
using FastEndpoints;
using WebApi2._0.Common;
using WebApi2._0.Domain.Entities;
using WebApi2._0.Domain.ValueObjects;
using WebApi2._0.Infrastructure.Persistence;

namespace WebApi2._0.Features.Orders.OrderLineItems;

[HttpGet("/{orderId}/orderlineitems")]
[Group<Orders>]
public sealed class GetOrderLineItemsEndpoint : Endpoint<GetOrderLineItemsRequest,
    PaginationResponse<OrderLineItemsListResponse>,
    OrderLineItemListResponseMapper>
{
    private readonly GiveAwayDbContext _dbContext;

    public GetOrderLineItemsEndpoint(GiveAwayDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public override async Task<PaginationResponse<OrderLineItemsListResponse>> ExecuteAsync(
        GetOrderLineItemsRequest req, CancellationToken ct)
    {
        Expression<Func<OrderLineItem, bool>> predicate = GetOrderLineItemsPredicate.GetPredicate(req);

        var query = _dbContext.OrderLineItems.AsQueryable();

        query = query.Where(predicate);

        var count = await query.CountAsync(ct);
        
        var data = await query
            .OrderByDescending(x=>x.CreatedDate)
            .Skip(PaginationUtils.GetSkip(req.Page, req.PageSize))
            .Take(PaginationUtils.GetTake(req.PageSize))
            .Select(x => new OrderLineItemsListResponse()
            {
                Condition = x.IndividualFashionItem.Condition,
                CreatedDate = x.CreatedDate,
                Quantity = x.Quantity,
                CategoryName = x.IndividualFashionItem.MasterItem.Category.Name,
                ItemBrand = x.IndividualFashionItem.MasterItem.Brand,
                OrderCode = x.Order.OrderCode,
                PaymentDate = x.PaymentDate,
                ItemCode = x.IndividualFashionItem.ItemCode,
                ItemColor = x.IndividualFashionItem.Color,
                ItemGender = x.IndividualFashionItem.MasterItem.Gender,
                ItemId = x.IndividualFashionItemId,
                ItemImage = x.IndividualFashionItem.Images.Select(img => img.Url).ToList(),
                ItemName = x.IndividualFashionItem.MasterItem.Name,
                ItemNote = x.IndividualFashionItem.Note,
                ItemSize = x.IndividualFashionItem.Size,
                ItemStatus = x.IndividualFashionItem.Status,
                ItemType = x.IndividualFashionItem.Type,
                ShopAddress = x.IndividualFashionItem.MasterItem.Shop.Address,
                ShopId = x.IndividualFashionItem.MasterItem.ShopId,
                UnitPrice = x.IndividualFashionItem.SellingPrice ?? 0,
                OrderLineItemId = x.OrderLineItemId,
                RefundExpirationDate = x.RefundExpirationDate,
                ReservedExpirationDate = x.ReservedExpirationDate
            })
            .ToListAsync(ct);

        var result = new PaginationResponse<OrderLineItemsListResponse>
        {
            Items = data,
            PageNumber = req.Page ?? -1,
            PageSize = req.PageSize ?? -1,
            TotalCount = count
        };

        return result;
    }
}