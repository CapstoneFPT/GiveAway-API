using System.Linq.Expressions;
using LinqKit;
using Microsoft.EntityFrameworkCore;
using WebApi2._0.Domain.Entities;

namespace WebApi2._0.Features.Accounts.Orders.GetOrders;

public static class GetAccountOrdersPredicates
{
    public static Expression<Func<Order, bool>> GetPredicate(GetAccountOrdersRequest request)
    {
        var predicates = new Dictionary<
            Func<GetAccountOrdersRequest, bool>,
            Func<GetAccountOrdersRequest, Expression<Func<Order, bool>>
            >>()
        {
            [x => x.OrderCode != null] = GetAccountOrdersCodePredicate,
            [x => x.PaymentMethods is { Length: > 0 }] = GetAccountOrdersPaymentMethodPredicate,
            [x => x.Email != null] = GetAccountOrdersEmailPredicate,
            [x => x.RecipientName != null] = GetAccountOrdersRecipientNamePredicate,
            [x => x.Statuses is { Length: > 0 }] = GetAccountOrdersStatusPredicate,
            [x => x.PurchaseTypes is { Length: > 0 }] = GetAccountOrdersPurchaseTypesPredicate,
            [x => x.Phone != null] = GetAccountOrdersPhonePredicate,
            [x => x.CustomerName != null] = GetAccountOrdersCustomerNamePredicate,
            [x => x.IsFromAuction != null] = GetIsFromAuctionPredicate,
            [x => x.ShopId != null] = GetAccountOrdersShopIdPredicate,
        };

        return predicates
            .Where(x => x.Key(request) == true)
            .Select(x => x.Value(request))
            .Aggregate(PredicateBuilder.New<Order>(true), (current, next) => current.And(next));
    }

    private static Expression<Func<Order, bool>> GetAccountOrdersShopIdPredicate(GetAccountOrdersRequest arg)
    {
        return order =>
            order.OrderLineItems.Any(orderLineItem =>
                orderLineItem.IndividualFashionItem.MasterItem.ShopId == arg.ShopId.Value);
    }

    private static Expression<Func<Order, bool>> GetIsFromAuctionPredicate(GetAccountOrdersRequest arg)
    {
        if (arg.IsFromAuction == true)
        {
            return order => order.BidId != null;
        }

        return order => order.BidId == null;
    }

    private static Expression<Func<Order, bool>> GetAccountOrdersCustomerNamePredicate(GetAccountOrdersRequest arg)
    {
        return order => order.Member != null && EF.Functions.ILike(order.Member.Fullname, $"%{arg.CustomerName}%");
    }

    private static Expression<Func<Order, bool>> GetAccountOrdersPhonePredicate(GetAccountOrdersRequest arg)
    {
        return order => order.Phone != null && EF.Functions.ILike(order.Phone, $"%{arg.Phone}%");
    }

    private static Expression<Func<Order, bool>> GetAccountOrdersPurchaseTypesPredicate(GetAccountOrdersRequest arg)
    {
        return order => arg.PurchaseTypes != null && arg.PurchaseTypes.Contains(order.PurchaseType);
    }

    private static Expression<Func<Order, bool>> GetAccountOrdersStatusPredicate(GetAccountOrdersRequest arg)
    {
        return order=> arg.Statuses != null && arg.Statuses.Contains(order.Status);
    }

    private static Expression<Func<Order, bool>> GetAccountOrdersRecipientNamePredicate(GetAccountOrdersRequest arg)
    {
        return order => order.RecipientName != null && EF.Functions.ILike(order.RecipientName, $"%{arg.RecipientName}%");
    }

    private static Expression<Func<Order, bool>> GetAccountOrdersEmailPredicate(GetAccountOrdersRequest arg)
    {
        return order => order.Email != null && EF.Functions.ILike(order.Email, $"%{arg.Email}%");
    }

    private static Expression<Func<Order, bool>> GetAccountOrdersPaymentMethodPredicate(GetAccountOrdersRequest arg)
    {
        return order => arg.PaymentMethods != null && arg.PaymentMethods.Contains(order.PaymentMethod);
    }

    private static Expression<Func<Order, bool>> GetAccountOrdersCodePredicate(GetAccountOrdersRequest arg)
    {
        return order => EF.Functions.ILike(order.OrderCode, $"%{arg.OrderCode}%");
    }
}