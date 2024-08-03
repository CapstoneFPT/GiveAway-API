using AutoMapper;
using AutoMapper.QueryableExtensions;
using BusinessObjects.Dtos.AuctionItems;
using BusinessObjects.Dtos.Commons;
using BusinessObjects.Dtos.FashionItems;
using BusinessObjects.Entities;
using Dao;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using Exception = System.Exception;

namespace Repositories.FashionItems
{
    public class FashionItemRepository : IFashionItemRepository
    {
        private readonly IMapper _mapper;
        private readonly GiveAwayDbContext _giveAwayDbContext;

        public FashionItemRepository(
            IMapper mapper, GiveAwayDbContext dbContext)
        {
            _mapper = mapper;
            _giveAwayDbContext = dbContext;
        }

        public bool CheckItemIsInOrder(Guid itemId, Guid? memberId)
        {
            var result =
                    _giveAwayDbContext.OrderDetails
                        .Any(orderDetail =>
                            orderDetail.FashionItemId == itemId && orderDetail.Order.MemberId == memberId &&
                            orderDetail.Order.Status == OrderStatus.AwaitingPayment)
                ;
            return memberId.HasValue && result;
        }

        public async Task<List<Guid>> GetOrderedItems(List<Guid> itemIds, Guid memberId)
        {
            return await _giveAwayDbContext.OrderDetails
                .Where(orderDetail =>
                    itemIds.Contains(orderDetail.FashionItemId.Value) &&
                    orderDetail.Order.MemberId == memberId &&
                    orderDetail.Order.Status == OrderStatus.AwaitingPayment)
                .Select(orderDetail => orderDetail.FashionItemId.Value)
                .Distinct()
                .ToListAsync();
        }

        public async Task<FashionItem> AddFashionItem(FashionItem request)
        {
            return await GenericDao<FashionItem>.Instance.AddAsync(request);
        }

        

        public async Task<(List<T> Items, int Page, int PageSize, int TotalCount)> GetFashionItemProjections<T>(
            int? page,
            int? pageSize, Expression<Func<FashionItem, bool>>? predicate, Expression<Func<FashionItem, T>>? selector)
        {
            var query = _giveAwayDbContext.FashionItems.AsQueryable();

            if (predicate != null)
            {
                query = query.Where(predicate);
            }

            var count = await query.CountAsync();

            var pageNum = page ?? -1;
            var pageSizeNum = pageSize ?? -1;

            if (pageNum > 0 && pageSizeNum > 0)
            {
                query = query.Skip((pageNum - 1) * pageSizeNum).Take(pageSizeNum);
            }

            List<T> items = new List<T>();
            if (selector != null)
            {
                items = await query.Select(selector).ToListAsync();
            }
            else
            {
                items = await query.Cast<T>().ToListAsync();
            }

            return (items, pageNum, pageSizeNum, count);
        }


        public async Task<FashionItem> GetFashionItemById(Guid id)
        {
            var query = await _giveAwayDbContext.FashionItems.AsQueryable()
                .Include(c => c.Shop)
                .Include(a => a.Category)
                .Include(b => b.Images)
                .FirstOrDefaultAsync(x => x.ItemId.Equals(id));
            return query;
        }

        public async Task<PaginationResponse<FashionItemDetailResponse>> GetItemByCategoryHierarchy(Guid id,
            AuctionFashionItemRequest request)
        {
            var listCate = new HashSet<Guid>();
            await GetCategoryIdsRecursive(id, listCate);
            if (listCate.Count == 0)
            {
                return new PaginationResponse<FashionItemDetailResponse>
                {
                    Items = new List<FashionItemDetailResponse>(),
                    PageSize = request.PageSize ?? -1,
                    TotalCount = 0,
                    SearchTerm = request.SearchTerm,
                    PageNumber = request.PageNumber ?? -1,
                };
            }

            var query = GenericDao<Category>.Instance.GetQueryable()
                .Where(c => listCate.Contains(c.CategoryId))
                .Include(c => c.FashionItems).ThenInclude(c => c.Images)
                .SelectMany(c => c.FashionItems)
                .AsNoTracking();

            if (request.Status != null)
            {
                query = query.Where(f => request.Status.Contains(f.Status));
            }

            if (request.Type != null)
            {
                query = query.Where(x => request.Type.Contains(x.Type));
            }

            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                query = query.Where(f => EF.Functions.ILike(f.Name, $"%{request.SearchTerm}%"));
            }

            if (request.ShopId != null)
            {
                query = query.Where(f => f.ShopId.Equals(request.ShopId));
            }

            var count = await query.CountAsync();
            var pageNum = request.PageNumber ?? -1;
            var pageSizeNum = request.PageSize ?? -1;


            if (pageNum > 0 && pageSizeNum > 0)
            {
                query = query.Skip((pageNum - 1) * pageSizeNum)
                    .Take(pageSizeNum);
            }

            var items = await query
                .Select(
                    f => new FashionItemDetailResponse
                    {
                        ItemId = f.ItemId,
                        Type = f.Type,
                        SellingPrice = f.SellingPrice.Value,
                        Name = f.Name,
                        Note = f.Note,
                        Description = f.Description,
                        Condition = f.Condition,
                        ShopAddress = f.Shop.Address,
                        ShopId = f.Shop.ShopId,
                        /*Consigner = f.ConsignSaleDetail.ConsignSale.Member.Fullname,*/
                        CategoryName = f.Category.Name,
                        Size = f.Size,
                        Color = f.Color,
                        Brand = f.Brand,
                        Gender = f.Gender,
                        Status = f.Status,
                        Images = f.Images.Select(c => c.Url).ToList()
                    }
                )
                .ToListAsync();

            var result = new PaginationResponse<FashionItemDetailResponse>
            {
                Items = items,
                PageSize = request.PageSize ?? -1,
                TotalCount = count,
                SearchTerm = request.SearchTerm,
                PageNumber = request.PageNumber ?? -1,
            };


            return result;
        }

        public async Task BulkUpdate(List<FashionItem> fashionItems)
        {
            await GenericDao<FashionItem>.Instance.UpdateRange(fashionItems);
        }

        public Task<List<FashionItem>> GetFashionItems(Expression<Func<FashionItem, bool>> predicate)
        {
            var queryable = GenericDao<FashionItem>.Instance
                .GetQueryable()
                .Where(predicate);

            var result = queryable.ToListAsync();

            return result;
        }

        public async Task UpdateFashionItems(List<FashionItem> fashionItems)
        {
            await GenericDao<FashionItem>.Instance.UpdateRange(fashionItems);
        }

        public async Task<FashionItem> UpdateFashionItem(FashionItem fashionItem)
        {
            _giveAwayDbContext.FashionItems.Update(fashionItem);
            await _giveAwayDbContext.SaveChangesAsync();
            return fashionItem;
        }

        private async Task GetCategoryIdsRecursive(Guid? id, HashSet<Guid> categoryIds)
        {
            if (!categoryIds.Add(id.Value)) return;

            var childCategories = await GenericDao<Category>.Instance.GetQueryable()
                .Where(c => c.ParentId == id && c.Status == CategoryStatus.Available)
                .Select(c => c.CategoryId)
                .ToListAsync();

            foreach (var childId in childCategories)
            {
                await GetCategoryIdsRecursive(childId, categoryIds);
            }
        }

        public async Task<List<Guid?>?> IsItemBelongShop(Guid shopId, List<Guid?> listItemId)
        {
            var listItemNotbelongshop = new List<Guid?>();
            var listItem = await GenericDao<FashionItem>.Instance.GetQueryable().Include(c => c.Shop)
                .Where(c => listItemId.Contains(c.ItemId)).ToListAsync();
            foreach (FashionItem item in listItem)
            {
                if (!item.ShopId.Equals(shopId))
                {
                    listItemNotbelongshop.Add(item.ItemId);
                }
            }

            return listItemNotbelongshop;
        }
    }
}