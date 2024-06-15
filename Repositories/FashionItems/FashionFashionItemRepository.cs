using BusinessObjects.Dtos.AuctionItems;
using BusinessObjects.Dtos.Commons;
using BusinessObjects.Dtos.FashionItems;
using BusinessObjects.Entities;
using Dao;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.FashionItems
{
    public class FashionFashionItemRepository : IFashionItemRepository
    {
        private readonly GenericDao<FashionItem> _fashionitemDao;

        public FashionFashionItemRepository(GenericDao<FashionItem> fashionitemDao)
        {
            _fashionitemDao = fashionitemDao;
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
                        Duration = x.ConsignSaleDetail.ConsignSale.ConsignDuration,
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
    }
}
