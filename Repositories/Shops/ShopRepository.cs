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
        private readonly GenericDao<Shop> _shop;

        public ShopRepository()
        {
            _shop = new GenericDao<Shop>();
        }

        public async Task<Shop> CreateShop(Shop shop)
        {
            _shop.AddAsync(shop);
            await _shop.SaveChangesAsync();
            return shop;
        }
    }
}
