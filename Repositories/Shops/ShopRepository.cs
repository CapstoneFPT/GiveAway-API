using BusinessObjects;
using BusinessObjects.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.Shops
{
    public class ShopRepository : IShopRepository
    {
        private readonly GiveAwayDbContext _dbContext;

        public ShopRepository()
        {
            _dbContext = new GiveAwayDbContext();
        }

        public async Task<Shop> CreateShop(Shop shop)
        {
            _dbContext.Add(shop);
            await _dbContext.SaveChangesAsync();
            return shop;
        }
    }
}
