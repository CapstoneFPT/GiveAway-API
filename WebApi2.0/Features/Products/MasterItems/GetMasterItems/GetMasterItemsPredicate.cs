using System.Linq.Expressions;
using LinqKit;
using Microsoft.EntityFrameworkCore;
using WebApi2._0.Domain.Entities;
using WebApi2._0.Domain.Enums;

namespace WebApi2._0.Features.Products.MasterItems.GetMasterItems;

public static class GetMasterItemsPredicate
{
    public static Expression<Func<MasterFashionItem, bool>> GetPredicate(GetMasterItemsRequest request)
    {
        var predicates = new Dictionary<
            Func<GetMasterItemsRequest, bool>,
            Func<GetMasterItemsRequest, Expression<Func<MasterFashionItem, bool>>>
        >()
        {
            [x => x.MasterItemName != null] = GetMasterItemNamePredicate,
            [x => x.MasterItemCode != null] = GetMasterItemCodePredicate,
            [x => x.Brand != null] = GetMasterItemBrandPredicate,
            [x => x.CategoryId != null] = GetMasterItemCategoryPredicate,
            [x => x.ShopId != null] = GetMasterItemShopPredicate,
            [x => x.Genders is {Length: > 0}] = GetMasterItemGenderPredicate,
            [x => x.IsConsignment != null] = GetMasterItemConsignmentPredicate,
            [x => x.IsLeftInStock != null] = GetMasterItemConsignmentLeftInStockPredicate,
            [x => x.IsForSale != null] = GetMasterItemForSalePredicate,
            [x => x.IsCategoryAvailable != null] = GetMasterItemCategoryAvailablePredicate
        };

        return predicates.Where(keyValuePair => keyValuePair.Key(request) == true)
            .Select(keyValuePair => keyValuePair.Value(request))
            .Aggregate(PredicateBuilder.New<MasterFashionItem>(true), (current, next) => current.And(next));
    }

    private static Expression<Func<MasterFashionItem, bool>> GetMasterItemCategoryAvailablePredicate(
        GetMasterItemsRequest arg)
    {
        if (arg.IsCategoryAvailable == true)
            return it => it.IndividualFashionItems.Any(c => c.Status == FashionItemStatus.Available);
        return it => it.IndividualFashionItems.All(c => c.Status != FashionItemStatus.Available);
    }

    private static Expression<Func<MasterFashionItem, bool>> GetMasterItemForSalePredicate(GetMasterItemsRequest arg)
    {
        if (arg.IsForSale == true)
            return it => it.IndividualFashionItems.Any(c =>
                c.Type != FashionItemType.ConsignedForAuction && c.Status == FashionItemStatus.Available);
        return it => it.IndividualFashionItems.All(c => c.Status != FashionItemStatus.Available);
    }

    private static Expression<Func<MasterFashionItem, bool>> GetMasterItemConsignmentLeftInStockPredicate(
        GetMasterItemsRequest arg)
    {
        if (arg.IsLeftInStock == true)
            return it =>
                it.IndividualFashionItems.Any(c => c.Status == FashionItemStatus.Available);

        return it => it.IndividualFashionItems.All(c => c.Status != FashionItemStatus.Available);
    }

    private static Expression<Func<MasterFashionItem, bool>> GetMasterItemConsignmentPredicate(
        GetMasterItemsRequest arg)
    {
        return x => x.IsConsignment == arg.IsConsignment;
    }


    private static Expression<Func<MasterFashionItem, bool>> GetMasterItemGenderPredicate(GetMasterItemsRequest arg)
    {
        return x => arg.Genders != null && arg.Genders.Contains(x.Gender);
    }

    private static Expression<Func<MasterFashionItem, bool>> GetMasterItemShopPredicate(GetMasterItemsRequest arg)
    {
        return x => x.ShopId == arg.ShopId;
    }

    private static Expression<Func<MasterFashionItem, bool>> GetMasterItemCategoryPredicate(GetMasterItemsRequest arg)
    {
        return x => x.CategoryId == arg.CategoryId;
    }

    private static Expression<Func<MasterFashionItem, bool>> GetMasterItemBrandPredicate(GetMasterItemsRequest arg)
    {
        return x => EF.Functions.ILike(x.Brand, $"%{arg.Brand}%");
    }

    private static Expression<Func<MasterFashionItem, bool>> GetMasterItemCodePredicate(GetMasterItemsRequest arg)
    {
        return x => EF.Functions.ILike(x.MasterItemCode, $"%{arg.MasterItemCode}%");
    }

    private static Expression<Func<MasterFashionItem, bool>> GetMasterItemNamePredicate(GetMasterItemsRequest arg)
    {
        return x => EF.Functions.ILike(x.Name, $"%{arg.MasterItemName}%");
    }
}