using System.Data.Entity;
using System.Linq.Expressions;
using FastEndpoints;
using WebApi2._0.Common;
using WebApi2._0.Domain.Entities;
using WebApi2._0.Domain.ValueObjects;
using WebApi2._0.Infrastructure.Persistence;

namespace WebApi2._0.Features.Orders.OrderLineItems;

[HttpGet("api/orders/{orderId}/orderlineitems")]
public class GetOrderLineItems : Endpoint<GetOrderLineItemsRequest, PaginationResponse<OrderLineItemsListResponse>,
    OrderLineItemListResponseMapper>
{
    private readonly GiveAwayDbContext _dbContext;

    public GetOrderLineItems(GiveAwayDbContext dbContext)
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

        query = query
                .Skip(PaginationUtils.GetSkip(req.Page, req.PageSize))
                .Take(PaginationUtils.GetTake(req.PageSize))
            ;

        var data = await query
            .Select(x => Map.FromEntity(x))
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