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
        private readonly GenericDao<FashionItem> _fashionitemDao;
        private readonly GenericDao<Category> _categoryDao;
        private readonly IMapper _mapper;

        public FashionItemRepository(GenericDao<FashionItem> fashionitemDao, GenericDao<Category> categoryDao,
            IMapper mapper)
        {
            _fashionitemDao = fashionitemDao;
            _categoryDao = categoryDao;
            _mapper = mapper;
        }

        public async Task<FashionItem> AddFashionItem(FashionItem request)
        {
            return await _fashionitemDao.AddAsync(request);
        }

        public async Task<PaginationResponse<FashionItemDetailResponse>> GetAllFashionItemPagination(
            AuctionFashionItemRequest request)
        {
            var query = _fashionitemDao.GetQueryable();
            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
                query = query.Where(x => EF.Functions.ILike(x.Name, $"%{request.SearchTerm}%"));
            if (request.Status != null)
            {
                query = query.Where(f => request.Status.Contains(f.Status));
            }

            if (request.Type != null)
            {
                query = query.Where(f => request.Type.Contains(f.Type));
            }

            if (request.ShopId != null)
            {
                query = query.Where(f => f.ShopId.Equals(request.ShopId));
            }

            if (request.GenderType != null)
            {
                query = query.Where(f => f.Gender.Equals(request.GenderType));
            }

            var count = await query.CountAsync();
            query = query.Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize);

            var items = await query
                .ProjectTo<FashionItemDetailResponse>(_mapper.ConfigurationProvider)
                .AsNoTracking().ToListAsync();

            var result = new PaginationResponse<FashionItemDetailResponse>
            {
                Items = items,
                PageSize = request.PageSize,
                TotalCount = count,
                SearchTerm = request.SearchTerm,
                PageNumber = request.PageNumber,
            };
            return result;
        }

        public async Task<FashionItem> GetFashionItemById(Guid id)
        {
            var query = await _fashionitemDao.GetQueryable()
                .Include(c => c.Shop)
                /*.Include(a => a.Category)
                .Include(b => b.ConsignSaleDetail).ThenInclude(c => c.ConsignSale).ThenInclude(c => c.Member)*/
                .AsNoTracking().FirstOrDefaultAsync(x => x.ItemId.Equals(id));
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
                    PageSize = request.PageSize,
                    TotalCount = 0,
                    SearchTerm = request.SearchTerm,
                    PageNumber = request.PageNumber,
                };
            }

            var query = _categoryDao.GetQueryable()
                .Where(c => listCate.Contains(c.CategoryId))
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

            var items = await query
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(
                    f => new FashionItemDetailResponse
                    {
                        ItemId = f.ItemId,
                        Type = f.Type,
                        SellingPrice = f.SellingPrice,
                        Name = f.Name,
                        Note = f.Note,
                        Value = f.Value,
                        Condition = f.Condition,
                        ShopAddress = f.Shop.Address,
                        ShopId = f.Shop.ShopId,
                        Consigner = f.ConsignSaleDetail.ConsignSale.Member.Fullname,
                        CategoryName = f.Category.Name,
                        Size = f.Size,
                        Color = f.Color,
                        Brand = f.Brand,
                        Gender = f.Gender,
                        Status = f.Status,
                    }
                )
                .ToListAsync();

            var result = new PaginationResponse<FashionItemDetailResponse>
            {
                Items = items,
                PageSize = request.PageSize,
                TotalCount = count,
                SearchTerm = request.SearchTerm,
                PageNumber = request.PageNumber,
            };


            return result;
        }

        public async Task BulkUpdate(List<FashionItem> fashionItems)
        {
            await _fashionitemDao.UpdateRange(fashionItems);
        }

        public Task<List<FashionItem>> GetFashionItems(Expression<Func<FashionItem, bool>> predicate)
        {
            var queryable = _fashionitemDao
                .GetQueryable()
                .Where(predicate);

            var result = queryable.ToListAsync();

            return result;
        }

        public async Task UpdateFashionItems(List<FashionItem> fashionItems)
        {
            await _fashionitemDao.UpdateRange(fashionItems);
        }

        public async Task<FashionItem> UpdateFashionItem(FashionItem fashionItem)
        {
            return await _fashionitemDao.UpdateAsync(fashionItem);
        }

        private async Task GetCategoryIdsRecursive(Guid? id, HashSet<Guid> categoryIds)
        {
            if (!categoryIds.Add(id.Value)) return;

            var childCategories = await _categoryDao.GetQueryable()
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
            var listItem = await _fashionitemDao.GetQueryable().Include(c => c.Shop)
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