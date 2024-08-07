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

        Task<IndividualFashionItem> GetFashionItemById(Guid id);
        Task<IndividualFashionItem> AddFashionItem(IndividualFashionItem request);
        Task<IndividualFashionItem> UpdateFashionItem(IndividualFashionItem fashionItem);

        Task<PaginationResponse<FashionItemDetailResponse>> GetItemByCategoryHierarchy(Guid id,
            AuctionFashionItemRequest request);

        Task BulkUpdate(List<IndividualFashionItem> fashionItems);
        Task<List<IndividualFashionItem>> GetFashionItems(Expression<Func<IndividualFashionItem, bool>> predicate);
        Task UpdateFashionItems(List<IndividualFashionItem> fashionItems);
        Task<List<Guid?>?> IsItemBelongShop(Guid shopId, List<Guid?> itemId);

        Task<(List<T> Items, int Page, int PageSize, int TotalCount)> GetFashionItemProjections<T>(
            int? page,
            int? pageSize, Expression<Func<IndividualFashionItem, bool>>? predicate, Expression<Func<IndividualFashionItem, T>>? selector);

        bool CheckItemIsInOrder(Guid itemId, Guid? memberId);
        Task<List<Guid>> GetOrderedItems(List<Guid> itemIds, Guid memberId);
    }
}