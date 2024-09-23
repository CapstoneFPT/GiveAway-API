using AutoMapper;
using AutoMapper.QueryableExtensions;
using BusinessObjects;
using BusinessObjects.Dtos.Commons;
using BusinessObjects.Dtos.Shops;
using BusinessObjects.Entities;
using Dao;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.Shops
{
    public class ShopRepository : IShopRepository
    {
        private readonly IMapper _mapper;
        private readonly GiveAwayDbContext _giveAwayDbContext;
        public ShopRepository(IMapper mapper, GiveAwayDbContext giveAwayDbContext)
        {
            _mapper = mapper;
            _giveAwayDbContext = giveAwayDbContext;
        }

        public async Task<Shop> CreateShop(Shop shop)
        {
            var result = await GenericDao<Shop>.Instance.AddAsync(shop);
            return result;
        }

        public async Task<List<ShopDetailResponse>> GetAllShop()
        {
            var listshop = await GenericDao<Shop>.Instance.GetQueryable().Where(c => c.Staff.Status.Equals(AccountStatus.Active))
                .ProjectTo<ShopDetailResponse>(_mapper.ConfigurationProvider)
                .AsNoTracking().ToListAsync();
            return listshop;
        }

        public async Task<ShopDetailResponse> GetShopByAccountId(Guid id)
        {
            var shop = await GenericDao<Shop>.Instance.GetQueryable()
                .Where(c => c.StaffId == id && c.Staff.Status.Equals(AccountStatus.Active))
                .ProjectTo<ShopDetailResponse>(_mapper.ConfigurationProvider)
                .AsNoTracking().FirstOrDefaultAsync();
            return shop!;
        }

        public async Task<ShopDetailResponse> GetShopById(Guid id)
        {
            var shop = await GenericDao<Shop>.Instance.GetQueryable()
                .Where(c => c.ShopId == id && c.Staff.Status.Equals(AccountStatus.Active))
                .ProjectTo<ShopDetailResponse>(_mapper.ConfigurationProvider)
                .AsNoTracking().FirstOrDefaultAsync();
            return shop!;
        }


        public async Task<Shop?> GetSingleShop(Expression<Func<Shop, bool>> predicate)
        {
            var result = await GenericDao<Shop>.Instance
                .GetQueryable()
                .SingleOrDefaultAsync(predicate);
            return result;
        }

        public async Task<string> GenerateShopCode()
        {
            int totalShopCode = 0;
            var listShop = await _giveAwayDbContext.Shops.AsQueryable()
                .ToListAsync();
            totalShopCode = listShop.Count + 1;
            string prefixInStock = new string($"GAS{totalShopCode}");
            return prefixInStock;
        }

        public Task<List<Shop>> GetShopEntities(Expression<Func<Shop, bool>>? predicate)
        {
            var query = _giveAwayDbContext.Shops.AsQueryable();

            if (predicate != null)
            {
                query = query.Where(predicate);
            }
            return query.ToListAsync();
        }

        public IQueryable<Shop> GetQueryable()
        {
            return GenericDao<Shop>.Instance.GetQueryable();
        }
    }
}