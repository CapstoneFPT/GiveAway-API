using BusinessObjects.Dtos.Commons;
using BusinessObjects.Dtos.Shops;
using Repositories.Shops;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Shops
{
    public class ShopService : IShopService
    {
        private readonly IShopRepository _shopRepository;

        public ShopService(IShopRepository shopRepository)
        {
            _shopRepository = shopRepository;
        }

        public async Task<Result<List<ShopDetailResponse>>> GetAllShop()
        {
            try
            {
                var response = new Result<List<ShopDetailResponse>>();
                var result = await _shopRepository.GetAllShop();
                if(result.Count() > 0)
                {
                    response.Data = result;
                    response.Messages = ["Successfully"];
                    response.ResultStatus = ResultStatus.Success;
                    return response;
                }
                response.Messages = ["There isn't any shop available"];
                response.ResultStatus = ResultStatus.Empty;
                return response;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}
