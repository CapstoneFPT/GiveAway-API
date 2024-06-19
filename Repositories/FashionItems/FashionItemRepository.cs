using BusinessObjects.Dtos.AuctionItems;
using BusinessObjects.Dtos.Commons;
using BusinessObjects.Dtos.FashionItems;
using BusinessObjects.Entities;
using Dao;
using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Asn1.Ocsp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace Repositories.FashionItems
{
    public class FashionItemRepository : IFashionItemRepository
    {
        private readonly GenericDao<FashionItem> _fashionitemDao;
        private readonly GenericDao<Category> _categoryDao;

        public FashionItemRepository(GenericDao<FashionItem> fashionitemDao, GenericDao<Category> categoryDao)
        {
            _fashionitemDao = fashionitemDao;
            _categoryDao = categoryDao;
        }

        public async Task<FashionItem> AddFashionItem(FashionItem request)
        {
            return await _fashionitemDao.AddAsync(request);
        }

        public async Task<PaginationResponse<FashionItemDetailResponse>> GetAllFashionItemPagination(AuctionFashionItemRequest request)
        {
            try
            {
                var query = _fashionitemDao.GetQueryable();
                if (!string.IsNullOrWhiteSpace(request.SearchTerm))
                    query = query.Where(x => EF.Functions.ILike(x.Name, $"%{request.SearchTerm}%"));

                query = query.Where(x => x.Status.Equals(FashionItemStatus.Available.ToString()) && x.Type.Equals(FashionItemType.ConsignedForSale.ToString()));

                var count = await query.CountAsync();
                query = query.Skip((request.PageNumber - 1) * request.PageSize)
                    .Take(request.PageSize);

                var items = await query
                    .Include(c => c.Shop)
                    .Include(a => a.Category)
                    .Include(b => b.ConsignSaleDetail).ThenInclude(c => c.ConsignSale).ThenInclude(c => c.Member)
                    .Select(x => new FashionItemDetailResponse
                    {
                        ItemId = x.ItemId,
                        Type = x.Type,
                        Name = x.Name,
                        SellingPrice = x.SellingPrice,
                        Note = x.Note,
                        Quantity = x.Quantity,
                        Value = x.Value,
                        Condition = x.Condition,
                        ConsignDuration = x.ConsignSaleDetail.ConsignSale.ConsignDuration,
                        StartDate = x.ConsignSaleDetail.ConsignSale.StartDate,
                        EndDate = x.ConsignSaleDetail.ConsignSale.EndDate,
                        ShopAddress = x.Shop.Address,
                        Consigner = x.ConsignSaleDetail.ConsignSale.Member.Fullname,
                        CategoryName = x.Category.Name,
                        Status = x.Status,
                    })
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
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<FashionItem> GetFashionItemById(Guid id)
        {
            try
            {
                var query = await _fashionitemDao.GetQueryable()
                    .Include(c => c.Shop)
                    .Include(a => a.Category)
                    .Include(b => b.ConsignSaleDetail).ThenInclude(c => c.ConsignSale).ThenInclude(c => c.Member)
                    .AsNoTracking().FirstOrDefaultAsync(x => x.ItemId.Equals(id));
                return query;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public  async Task<PaginationResponse<FashionItemDetailResponse>> GetItemByCategoryHierarchy(Guid id, AuctionFashionItemRequest request)
        {
            var listCate = await _categoryDao.GetQueryable().Where(c => c.ParentId == id).Select(c => c.CategoryId).ToListAsync();

            var listitem = await _categoryDao.GetQueryable()
                .Include(c => c.FashionItems).ThenInclude(c => c.ConsignSaleDetail).ThenInclude(c => c.ConsignSale)
                .ThenInclude(c => c.Member)
                .ThenInclude(c => c.Shop)
                .Where(c => listCate.Contains(c.CategoryId))
                .SelectMany(a => a.FashionItems.Where(c => c.Status.Equals(FashionItemStatus.Available.ToString())).Select(c => new FashionItemDetailResponse
                {
                    ItemId = c.ItemId,
                    Type = c.Type,
                    SellingPrice = c.SellingPrice,
                    Name = c.Name,
                    Note = c.Note,
                    Quantity = c.Quantity,
                    Value = c.Value,
                    Condition = c.Condition,
                    ConsignDuration = c.ConsignSaleDetail.ConsignSale.ConsignDuration,
                    Status = c.Status,
                    StartDate = c.ConsignSaleDetail.ConsignSale.StartDate,
                    EndDate = c.ConsignSaleDetail.ConsignSale.EndDate,
                    ShopAddress = c.Shop.Address,
                    Consigner = c.ConsignSaleDetail.ConsignSale.Member.Fullname,
                    CategoryName = c.Category.Name,
                }))
                .AsNoTracking().ToListAsync();
            

            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
                listitem = listitem.Where(x => x.Name.Contains(request.SearchTerm)).ToList();

            var count = listitem.Count();
            listitem = listitem.Skip((request.PageNumber -1) * request.PageSize).Take(request.PageSize).ToList();

            var result = new PaginationResponse<FashionItemDetailResponse>
            {
                Items = listitem,
                PageSize = request.PageSize,
                TotalCount = count,
                SearchTerm = request.SearchTerm,
                PageNumber = request.PageNumber,
            };

            
            return result;
        }

        public async Task<FashionItem> UpdateFashionItem(FashionItem fashionItem)
        {
            return await _fashionitemDao.UpdateAsync(fashionItem);
        }
        private void GetCategoryIdsRecursive(Category category, List<Guid> categoryIds)
        {
            categoryIds.Add(category.CategoryId);

            foreach (var child in category.Children)
            {
                GetCategoryIdsRecursive(child, categoryIds);
            }
        }
    }
}
