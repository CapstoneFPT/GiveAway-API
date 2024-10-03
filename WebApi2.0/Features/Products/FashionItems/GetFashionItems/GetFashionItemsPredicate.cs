using System.Linq.Expressions;
using LinqKit;
using Microsoft.EntityFrameworkCore;
using WebApi2._0.Domain.Entities;

namespace WebApi2._0.Features.Products.FashionItems.GetFashionItems;

public static class GetFashionItemsPredicate
{
    public static Expression<Func<IndividualFashionItem, bool>> GetPredicate(GetFashionItemsRequest request)
    {
        var predicates =
            new Dictionary<
                Func<GetFashionItemsRequest, bool>,
                Func<GetFashionItemsRequest, Expression<Func<IndividualFashionItem, bool>>>>()
            {
                { req => req.ItemCode != null, GetItemCodePredicate },
                { req => req.Gender != null, GetGenderPredicate },
                { req => req.Color != null, GetColorPredicate },
                { req => req.Size != null, GetSizePredicate },
                { req => req.Condition != null, GetConditionPredicate },
                { req => req.MinPrice != null, GetPriceRangePredicate },
                { req => req.MaxPrice != null, GetPriceRangePredicate },
                { req => req.Statuses is { Length: > 0 }, GetStatusPredicate },
                { req => req.Types is { Length: > 0 }, GetTypePredicate },
                { req => req.Name != null, GetNamePredicate },
                { req => req.CategoryId != null, GetCategoryIdPredicate },
                { req => req.ShopId != null, GetShopIdPredicate },
                { req => request.MasterItemId != null, GetMasterItemIdPredicate },
                { req => req.MasterItemCode != null, GetMasterItemCodePredicate }
            };

        return predicates
            .Where(keyValuePair => keyValuePair.Key(request) == true)
            .Select(keyValuePair => keyValuePair.Value(request))
            .Aggregate(PredicateBuilder.New<IndividualFashionItem>(true), (current, next) => current.And(next));
    }

    private static Expression<Func<IndividualFashionItem, bool>> GetItemCodePredicate(GetFashionItemsRequest request)
    {
        return item => EF.Functions.ILike(item.ItemCode, $"%{request.ItemCode}%");
    }

    private static Expression<Func<IndividualFashionItem, bool>> GetGenderPredicate(GetFashionItemsRequest request)
    {
        return item => item.MasterItem.Gender == request.Gender;
    }

    private static Expression<Func<IndividualFashionItem, bool>> GetColorPredicate(GetFashionItemsRequest request)
    {
        return item => EF.Functions.ILike(item.Color, $"%{request.Color}%");
    }

    private static Expression<Func<IndividualFashionItem, bool>> GetSizePredicate(GetFashionItemsRequest request)
    {
        return item => item.Size == request.Size;
    }

    private static Expression<Func<IndividualFashionItem, bool>> GetConditionPredicate(GetFashionItemsRequest request)
    {
        return item => EF.Functions.ILike(item.Condition, $"%{request.Condition}%");
    }

    private static Expression<Func<IndividualFashionItem, bool>> GetPriceRangePredicate(GetFashionItemsRequest request)
    {
        return item => (request.MinPrice == null || item.SellingPrice >= request.MinPrice) &&
                       (request.MaxPrice == null || item.SellingPrice <= request.MaxPrice);
    }

    private static Expression<Func<IndividualFashionItem, bool>> GetStatusPredicate(GetFashionItemsRequest request)
    {
        return item => request.Statuses != null && request.Statuses.Contains(item.Status);
    }

    private static Expression<Func<IndividualFashionItem, bool>> GetTypePredicate(GetFashionItemsRequest request)
    {
        return item => request.Types != null && request.Types.Contains(item.Type);
    }

    private static Expression<Func<IndividualFashionItem, bool>> GetNamePredicate(GetFashionItemsRequest request)
    {
        return item => EF.Functions.ILike(item.MasterItem.Name, $"%{request.Name}%");
    }

    private static Expression<Func<IndividualFashionItem, bool>> GetCategoryIdPredicate(GetFashionItemsRequest request)
    {
        return item => item.MasterItem.CategoryId == request.CategoryId;
    }

    private static Expression<Func<IndividualFashionItem, bool>> GetShopIdPredicate(GetFashionItemsRequest request)
    {
        return item => item.MasterItem.ShopId == request.ShopId;
    }

    private static Expression<Func<IndividualFashionItem, bool>> GetMasterItemIdPredicate(
        GetFashionItemsRequest request)
    {
        return item => item.MasterItemId == request.MasterItemId;
    }

    private static Expression<Func<IndividualFashionItem, bool>> GetMasterItemCodePredicate(
        GetFashionItemsRequest request)
    {
        return item => EF.Functions.ILike(item.MasterItem.MasterItemCode, $"%{request.MasterItemCode}%");
    }
}