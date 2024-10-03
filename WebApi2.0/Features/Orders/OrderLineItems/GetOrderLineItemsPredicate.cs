using System.Linq.Expressions;
using LinqKit;
using Microsoft.EntityFrameworkCore;
using WebApi2._0.Domain.Entities;

namespace WebApi2._0.Features.Orders.OrderLineItems;

public static class GetOrderLineItemsPredicate
{
    public static Expression<Func<OrderLineItem, bool>> GetPredicate(GetOrderLineItemsRequest request)
    {
        var predicates =
            new Dictionary<
                Func<GetOrderLineItemsRequest, bool>,
                Func<GetOrderLineItemsRequest, Expression<Func<OrderLineItem, bool>>>
            >()
            {
                { req => req.OrderId != null, GetOrderIdPredicate },
                { req => req.OrderLineItemId.HasValue, GetOrderLineItemIdPredicate },
                { req => req.OrderCode.HasValue, GetOrderCodePredicate }
            };

        return predicates
            .Where(x => x.Key(request))
            .Select(x => x.Value(request))
            .Aggregate(PredicateBuilder.New<OrderLineItem>(true), (current, next) => current.And(next));
    }

    private static Expression<Func<OrderLineItem, bool>> GetOrderCodePredicate(GetOrderLineItemsRequest arg)
    {
        return item => EF.Functions.ILike(item.Order.OrderCode, $"%{arg.OrderCode}%");
    }

    private static Expression<Func<OrderLineItem, bool>> GetOrderLineItemIdPredicate(GetOrderLineItemsRequest arg)
    {
        return item => item.OrderLineItemId == arg.OrderLineItemId;
    }

    private static Expression<Func<OrderLineItem, bool>> GetOrderIdPredicate(GetOrderLineItemsRequest arg)
    {
        return item => item.OrderId == arg.OrderId;
    }
}