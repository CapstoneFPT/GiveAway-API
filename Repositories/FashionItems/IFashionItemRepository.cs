using BusinessObjects.Dtos.AuctionItems;
using BusinessObjects.Dtos.Commons;
using BusinessObjects.Dtos.FashionItems;
using BusinessObjects.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.FashionItems
{
    public interface IFashionItemRepository
    {
        // Task<PaginationResponse<FashionItemDetailResponse>> GetAllFashionItemPagination(
        //     AuctionFashionItemRequest request);

        Task<IndividualFashionItem> GetFashionItemById(Expression<Func<IndividualFashionItem, bool>> predicate);
        Task<IndividualFashionItem> AddInvidualFashionItem(IndividualFashionItem request);
        Task<MasterFashionItem> AddSingleMasterFashionItem(MasterFashionItem request);
        // Task<List<MasterFashionItemShop>> AddRangeMasterFashionItemShop(List<MasterFashionItemShop> request);
        // Task<FashionItemVariation> AddSingleFashionItemVariation(FashionItemVariation request);
        Task<IndividualFashionItem> UpdateFashionItem(IndividualFashionItem fashionItem);
        Task<bool?> IsConsignEnded(Guid? itemId);
        Task<PaginationResponse<FashionItemDetailResponse>> GetItemByCategoryHierarchy(Guid id,
            AuctionFashionItemRequest request);

        Task BulkUpdate(List<IndividualFashionItem> fashionItems);
        Task<List<IndividualFashionItem>> GetFashionItems(Expression<Func<IndividualFashionItem, bool>> predicate);
        Task UpdateFashionItems(List<IndividualFashionItem> fashionItems);
        Task UpdateMasterItem(MasterFashionItem masterFashionItem);
        Task<List<Guid>> IsItemBelongShop(Guid shopId, List<Guid> itemId);

        Task<(List<T> Items, int Page, int PageSize, int TotalCount)> GetIndividualItemProjections<T>(
            int? page,
            int? pageSize, Expression<Func<IndividualFashionItem, bool>>? predicate, Expression<Func<IndividualFashionItem, T>>? selector);
        Task<(List<T> Items, int Page, int PageSize, int TotalCount)> GetMasterItemProjections<T>(
            int? page,
            int? pageSize, Expression<Func<MasterFashionItem, bool>>? predicate, Expression<Func<MasterFashionItem, T>>? selector);
        /*Task<(List<T> Items, int Page, int PageSize, int TotalCount)> GetFashionItemVariationProjections<T>(
            int? page,
            int? pageSize, Expression<Func<FashionItemVariation, bool>>? predicate, Expression<Func<FashionItemVariation, T>>? selector);*/
        Task<MasterFashionItem?> GetSingleMasterItem(Expression<Func<MasterFashionItem, bool>> predicate);
        /*Task<FashionItemVariation?> GetSingleFashionItemVariation(Expression<Func<FashionItemVariation?, bool>> predicate);*/
        bool CheckItemIsInOrder(Guid itemId, Guid? memberId);
        bool CheckIsItemConsigned(Guid itemId, Guid? memberId);
        Task<List<Guid>> GetOrderedItems(List<Guid> itemIds, Guid memberId);
        Task<string> GenerateMasterItemCode(string itemCode);
        Task<string> GenerateIndividualItemCode(string masterItemCode);
        Task<string> GenerateConsignMasterItemCode(string itemCode, Guid shopId);
        IQueryable<IndividualFashionItem> GetIndividualQueryable();
        IQueryable<MasterFashionItem> GetMasterQueryable();
        Task<bool> DeleteRangeIndividualItems(List<IndividualFashionItem> fashionItems);
    }
}