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
        Task<PaginationResponse<FashionItemDetailResponse>> GetAllFashionItemPagination(AuctionFashionItemRequest request);
        Task<FashionItem> GetFashionItemById(Guid id);
        Task<FashionItem> AddFashionItem(FashionItem request);
        Task<FashionItem> UpdateFashionItem(FashionItem fashionItem);
        Task<PaginationResponse<FashionItemDetailResponse>> GetItemByCategoryHierarchy(Guid id, AuctionFashionItemRequest request);
        Task BulkUpdate(List<FashionItem> fashionItems);
        Task<List<FashionItem>> GetFashionItems(Expression<Func<FashionItem,bool>> predicate);
        Task UpdateFashionItems(List<FashionItem> fashionItems);
        Task<List<Guid?>?> IsItemBelongShop(Guid shopId, List<Guid?> itemId);
    }
}
