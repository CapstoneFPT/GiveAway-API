using BusinessObjects;
using BusinessObjects.Entities;
using Dao;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.Shops
{
    public class ShopRepository : IShopRepository
    {
        private readonly GenericDao<Shop> _shopDao;

        public ShopRepository()
        {
            _shopDao = new GenericDao<Shop>();
        }

        public async Task<Shop> CreateShop(Shop shop)
        {
            var result = await _shopDao.AddAsync(shop);
            return result;
        }
    }
}
