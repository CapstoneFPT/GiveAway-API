using BusinessObjects.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.Shops
{
    public interface IShopRepository
    {
        Task<Shop> CreateShop(Shop shop);
    }
}
