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
        private static HashSet<string> generatedStrings = new HashSet<string>();
        private static readonly string? num = null;
        /*private readonly string prefixInStock;*/
        public FashionItemRepository(
            IMapper mapper, GiveAwayDbContext dbContext)
        {
            _mapper = mapper;
            _giveAwayDbContext = dbContext;
        }
        public string GenerateItemCodeForShop(string shopCode, string itemCode)
        {
            string prefixInStock = new string($"CS-{shopCode}-{itemCode}");
            return prefixInStock;
        }

        public async Task<(List<T> Items, int Page, int PageSize, int TotalCount)> GetMasterItemProjections<T>(int? page, int? pageSize, Expression<Func<MasterFashionItem, bool>>? predicate, Expression<Func<MasterFashionItem, T>>? selector)
        {
            var query = _giveAwayDbContext.MasterFashionItems.AsQueryable();

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

        public async Task<(List<T> Items, int Page, int PageSize, int TotalCount)> GetFashionItemVariationProjections<T>(int? page, int? pageSize, Expression<Func<FashionItemVariation, bool>>? predicate, Expression<Func<FashionItemVariation, T>>? selector)
        {
            var query = _giveAwayDbContext.FashionItemVariations.AsQueryable();

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

        public async Task<MasterFashionItem?> GetSingleMasterItem(Expression<Func<MasterFashionItem, bool>> predicate)
        {
            return await GenericDao<MasterFashionItem>.Instance.GetQueryable()
                .Include(c => c.Images)
                .Include(c => c.Shop)
                .Where(predicate)
                .FirstOrDefaultAsync();
        }

        public async Task<FashionItemVariation?> GetSingleFashionItemVariation(Expression<Func<FashionItemVariation?, bool>> predicate)
        {
            return await GenericDao<FashionItemVariation>.Instance.GetQueryable()
                .Include(c => c.MasterItem)
                .Where(predicate)
                .FirstOrDefaultAsync();
        }

        public bool CheckItemIsInOrder(Guid itemId, Guid? memberId)
        {
            var result =
                    _giveAwayDbContext.OrderDetails
                        .Any(orderDetail =>
                            orderDetail.IndividualFashionItemId == itemId && orderDetail.Order.MemberId == memberId &&
                            orderDetail.Order.Status == OrderStatus.AwaitingPayment)
                ;
            return memberId.HasValue && result;
        }

        public async Task<List<Guid>> GetOrderedItems(List<Guid> itemIds, Guid memberId)
        {
            return await _giveAwayDbContext.OrderDetails
                .Where(orderDetail =>
                    itemIds.Contains(orderDetail.IndividualFashionItemId.Value) &&
                    orderDetail.Order.MemberId == memberId &&
                    orderDetail.Order.Status == OrderStatus.AwaitingPayment)
                .Select(orderDetail => orderDetail.IndividualFashionItemId.Value)
                .Distinct()
                .ToListAsync();
        }

        public async Task<string> GenerateMasterItemCode(string itemCode)
        {
            int totalMasterCode = 0;
            var listMasterItemCode = await _giveAwayDbContext.MasterFashionItems.AsQueryable()
                .Where(c => c.MasterItemCode.Contains(itemCode))
                .Select(c => c.MasterItemCode).ToListAsync();
            totalMasterCode = listMasterItemCode.Count + 1;
            string prefixInStock = new string($"IS-GAS-{itemCode.ToUpper()}{totalMasterCode}");
            return prefixInStock;
        }

        public async Task<string> GenerateIndividualItemCode(Guid masterItemId)
        {
            int itemNumber = 1;
            int totalItemNumber = 0;
            var masterItem = await _giveAwayDbContext.MasterFashionItems.AsQueryable()
                .Include(c => c.Variations)
                .ThenInclude(c => c.IndividualItems)
                .Where(c => c.MasterItemId == masterItemId)
                .FirstOrDefaultAsync();
            string prefix = new string($"{masterItem.MasterItemCode}{itemNumber}");
            var individualItems = await _giveAwayDbContext.IndividualFashionItems
                .Where(item => item.Variation!.MasterItemId == masterItemId)
                .ToListAsync();
            totalItemNumber = individualItems.Count + 1;
            prefix = new string($"{masterItem.MasterItemCode}{totalItemNumber}");
            
            return prefix;
        }

        public async Task<IndividualFashionItem> AddInvidualFashionItem(IndividualFashionItem request)
        {
            return await GenericDao<IndividualFashionItem>.Instance.AddAsync(request);
        }

        public async Task<MasterFashionItem> AddSingleMasterFashionItem(MasterFashionItem request)
        {
            return await GenericDao<MasterFashionItem>.Instance.AddAsync(request);
        }
        
        

        public async Task<FashionItemVariation> AddSingleFashionItemVariation(FashionItemVariation request)
        {
            return await GenericDao<FashionItemVariation>.Instance.AddAsync(request);
        }


        public async Task<(List<T> Items, int Page, int PageSize, int TotalCount)> GetIndividualItemProjections<T>(
            int? page,
            int? pageSize, Expression<Func<IndividualFashionItem, bool>>? predicate, Expression<Func<IndividualFashionItem, T>>? selector)
        {
            var query = _giveAwayDbContext.IndividualFashionItems.AsQueryable();

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


        public async Task<IndividualFashionItem> GetFashionItemById(Expression<Func<IndividualFashionItem, bool>> predicate)
        {
            var query = await _giveAwayDbContext.IndividualFashionItems.AsQueryable()
                .Include(c => c.Variation!.MasterItem.Shop)
                .Include(a => a.Variation.MasterItem.Category)
                .Include(b => b.Images)
                .Where(predicate)
                .FirstOrDefaultAsync();
            return query;
        }

        public async Task<FashionItemVariation> UpdateFashionItemVariation(FashionItemVariation fashionItemVariation)
        {
            return await GenericDao<FashionItemVariation>.Instance.UpdateAsync(fashionItemVariation);
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
                .Include(c => c.MasterFashionItems).ThenInclude(c => c.Images)
                .SelectMany(c => c.MasterFashionItems)
                .AsNoTracking();
            // query = query.OrderByDescending(c => c.CreatedDate);
            // if (request.Status != null)
            // {
            //     query = query.Where(f => request.Status.Contains(f.));
            // }
            //
            // if (request.Type != null)
            // {
            //     query = query.Where(x => request.Type.Contains(x.Type));
            // }
            //
            // if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            // {
            //     query = query.Where(f => EF.Functions.ILike(f.Name, $"%{request.SearchTerm}%"));
            // }
            //
            // if (request.ShopId != null)
            // {
            //     query = query.Where(f => f.ShopId.Equals(request.ShopId));
            // }

            var count = await query.CountAsync();
            var pageNum = request.PageNumber ?? -1;
            var pageSizeNum = request.PageSize ?? -1;


            if (pageNum > 0 && pageSizeNum > 0)
            {
                query = query.Skip((pageNum - 1) * pageSizeNum)
                    .Take(pageSizeNum);
            }

            // var items = await query
            //     .Select(
            //         f => new FashionItemDetailResponse
            //         {
            //             ItemId = f.,
            //             Type = f.Type,
            //             SellingPrice = f.SellingPrice.Value,
            //             Name = f.Name,
            //             Note = f.Note,
            //             Description = f.Description,
            //             Condition = f.Condition,
            //             ShopAddress = f.Shop.Address,
            //             ShopId = f.Shop.ShopId,
            //             /*Consigner = f.ConsignSaleDetail.ConsignSale.Member.Fullname,*/
            //             CategoryName = f.Category.Name,
            //             Size = f.Size,
            //             Color = f.Color,
            //             Brand = f.Brand,
            //             Gender = f.Gender,
            //             Status = f.Status,
            //             Images = f.Images.Select(c => c.Url).ToList()
            //         }
            //     )
            //     .ToListAsync();

            var result = new PaginationResponse<FashionItemDetailResponse>
            {
                Items = [],
                PageSize = request.PageSize ?? -1,
                TotalCount = count,
                SearchTerm = request.SearchTerm,
                PageNumber = request.PageNumber ?? -1,
            };


            return result;
        }

        public async Task BulkUpdate(List<IndividualFashionItem> fashionItems)
        {
            await GenericDao<IndividualFashionItem>.Instance.UpdateRange(fashionItems);
        }

        public Task<List<IndividualFashionItem>> GetFashionItems(Expression<Func<IndividualFashionItem, bool>> predicate)
        {
            var queryable = GenericDao<IndividualFashionItem>.Instance
                .GetQueryable()
                .Where(predicate);

            var result = queryable.ToListAsync();

            return result;
        }

        public async Task UpdateFashionItems(List<IndividualFashionItem> fashionItems)
        {
            await GenericDao<IndividualFashionItem>.Instance.UpdateRange(fashionItems);
        }

        public async Task<IndividualFashionItem> UpdateFashionItem(IndividualFashionItem fashionItem)
        {
            _giveAwayDbContext.IndividualFashionItems.Update(fashionItem);
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
            var listItem = await GenericDao<IndividualFashionItem>.Instance.GetQueryable()
                // .Include(c => c.Shop)
                .Where(c => listItemId.Contains(c.ItemId)).ToListAsync();
            foreach (IndividualFashionItem item in listItem)
            {
                // if (!item.ShopId.Equals(shopId))
                // {
                //     listItemNotbelongshop.Add(item.ItemId);
                // }
            }

            return listItemNotbelongshop;
        }
    }
}