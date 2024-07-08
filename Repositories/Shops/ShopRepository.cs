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
        private readonly GenericDao<Shop> _shopDao;
        private readonly IMapper _mapper;

        public ShopRepository(GenericDao<Shop> shopDao, IMapper mapper)
        {
            _shopDao = shopDao;
            _mapper = mapper;
        }

        public async Task<Shop> CreateShop(Shop shop)
        {
            var result = await _shopDao.AddAsync(shop);
            return result;
        }

        public async Task<List<ShopDetailResponse>> GetAllShop()
        {
            var listshop = await _shopDao.GetQueryable().Where(c => c.Staff.Status.Equals(AccountStatus.Active))
                .ProjectTo<ShopDetailResponse>(_mapper.ConfigurationProvider)
                .AsNoTracking().ToListAsync();
            return listshop;
        }

        public async Task<ShopDetailResponse> GetShopByAccountId(Guid id)
        {
            var shop = await _shopDao.GetQueryable()
                .Where(c => c.StaffId == id && c.Staff.Status.Equals(AccountStatus.Active))
                .ProjectTo<ShopDetailResponse>(_mapper.ConfigurationProvider)
                .AsNoTracking().FirstOrDefaultAsync();
            return shop;
        }

        public async Task<ShopDetailResponse> GetShopById(Guid id)
        {
            var shop = await _shopDao.GetQueryable()
                .Where(c => c.ShopId == id && c.Staff.Status.Equals(AccountStatus.Active))
                .ProjectTo<ShopDetailResponse>(_mapper.ConfigurationProvider)
                .AsNoTracking().FirstOrDefaultAsync();
            return shop;
        }


        public async Task<Shop?> GetSingleShop(Expression<Func<Shop, bool>> predicate)
        {
            try
            {
                var result = await _shopDao
                    .GetQueryable()
                    .SingleOrDefaultAsync(predicate);
                return result;
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }
    }
}