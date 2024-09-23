using BusinessObjects.Dtos.Shops;
using BusinessObjects.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.Shops
{
    public interface IShopRepository
    {
        Task<Shop> CreateShop(Shop shop);
        Task<List<ShopDetailResponse>> GetAllShop();
        Task<ShopDetailResponse> GetShopByAccountId(Guid id);
        Task<ShopDetailResponse> GetShopById(Guid id);
        Task<Shop?> GetSingleShop(Expression<Func<Shop, bool>> predicate);
        Task<string> GenerateShopCode();
        Task<List<Shop>> GetShopEntities(Expression<Func<Shop,bool>>? predicate);
        IQueryable<Shop> GetQueryable();
    }
}
