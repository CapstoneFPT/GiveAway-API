using BusinessObjects.Dtos.Commons;
using BusinessObjects.Dtos.Shops;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Shops
{
    public interface IShopService
    {
        Task<Result<List<ShopDetailResponse>>> GetAllShop();
    }
}
