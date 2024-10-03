using System.Linq.Expressions;
using LinqKit;
using Microsoft.EntityFrameworkCore;
using WebApi2._0.Domain.Entities;

namespace WebApi2._0.Features.Orders.GetOrders;

public static class GetOrdersPredicate
{
    public static Expression<Func<Order, bool>> GetPredicate(GetOrdersRequest request)
    {
        var predicates = new Dictionary<
            Func<GetOrdersRequest, bool>,
            Func<GetOrdersRequest, Expression<Func<Order, bool>>>
        >()
        {
            { req => !string.IsNullOrEmpty(req.Email), GetEmailPredicate },
            { req => !string.IsNullOrEmpty(req.CustomerName), GetCustomerNamePredicate },
            { req => !string.IsNullOrEmpty(req.OrderCode), GetOrderCodePredicate },
            { req => req.Statuses.Length > 0, GetStatusesPredicate },
            { req => req.PaymentMethods.Length > 0, GetPaymentMethodsPredicate },
            { req => req.PurchaseTypes.Length > 0, GetPurchaseTypesPredicate },
            { req => !string.IsNullOrEmpty(req.Phone), GetPhonePredicate },
            { req => req.ShopId.HasValue, GetShopIdPredicate },
            { req => !string.IsNullOrEmpty(req.RecipientName), GetRecipientNamePredicate },
            { req => request.IsFromAuction != null, GetIsFromAuctionPredicate }
        };

        return predicates
            .Where(keyValuePair => keyValuePair.Key(request) == true)
            .Select(keyValuePair => keyValuePair.Value(request))
            .Aggregate(PredicateBuilder.New<Order>(true), (current, next) => current.And(next));
    }

    private static Expression<Func<Order, bool>> GetEmailPredicate(GetOrdersRequest request) =>
        x => x.Email != null && EF.Functions.ILike(x.Email, $"%{request.Email}%");

    private static Expression<Func<Order, bool>> GetCustomerNamePredicate(GetOrdersRequest request)
        => x => x.Member != null && EF.Functions.ILike(x.Member.Fullname, 
            $"%{request.CustomerName}%");

    private static Expression<Func<Order, bool>> GetOrderCodePredicate(GetOrdersRequest request)
        => x => EF.Functions.ILike(x.OrderCode, $"%{request.OrderCode}%");

    private static Expression<Func<Order, bool>> GetStatusesPredicate(GetOrdersRequest request)
        => x => request.Statuses.Contains(x.Status);

    private static Expression<Func<Order, bool>> GetPaymentMethodsPredicate(GetOrdersRequest request)
        => x => request.PaymentMethods.Contains(x.PaymentMethod);

    private static Expression<Func<Order, bool>> GetPurchaseTypesPredicate(GetOrdersRequest request)
        => x => request.PurchaseTypes.Contains(x.PurchaseType);

    private static Expression<Func<Order, bool>> GetPhonePredicate(GetOrdersRequest request)
        => x => x.Phone != null && EF.Functions.ILike(x.Phone, $"%{request.Phone}%");

    private static Expression<Func<Order, bool>> GetShopIdPredicate(GetOrdersRequest request)
        => x => x.OrderLineItems.Any(c => c.IndividualFashionItem.MasterItem.ShopId == request.ShopId);

    private static Expression<Func<Order, bool>> GetRecipientNamePredicate(GetOrdersRequest request)
        => x => x.RecipientName != null && EF.Functions.ILike(x.RecipientName, 
            $"%{request.RecipientName}%");

    private static Expression<Func<Order, bool>> GetIsFromAuctionPredicate(GetOrdersRequest request)
        => request.IsFromAuction switch
        {
            true => x => x.BidId != null,
            false => x => x.BidId == null,
            _ => PredicateBuilder.New<Order>()
        };
}