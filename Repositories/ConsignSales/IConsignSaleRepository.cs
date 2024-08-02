using BusinessObjects.Dtos.Commons;
using BusinessObjects.Dtos.ConsignSales;
using BusinessObjects.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.ConsignSales
{
    public interface IConsignSaleRepository
    {
        Task<PaginationResponse<ConsignSaleResponse>> GetAllConsignSale(Guid accountId, ConsignSaleRequest request);
        Task<ConsignSaleResponse> GetConsignSaleById(Guid consignId);
        Task<ConsignSaleResponse> CreateConsignSale(Guid accountId, CreateConsignSaleRequest request);
        Task<ConsignSaleResponse> ApprovalConsignSale(Guid consignId, ConsignSaleStatus status);
        Task<List<ConsignSale>> GetAllConsignPendingByAccountId(Guid accountId,bool isTracking = false);
        Task<ConsignSaleResponse> ConfirmReceivedFromShop(Guid consignId);
        Task<ConsignSaleResponse> CreateConsignSaleByShop(Guid shopId, CreateConsignSaleByShopRequest request);
        Task<PaginationResponse<ConsignSaleResponse>> GetAllConsignSaleByShopId(Guid shopId, ConsignSaleRequestForShop request);
        Task<ConsignSale?> GetSingleConsignSale(Expression<Func<ConsignSale, bool>> predicate);
        Task UpdateConsignSale(ConsignSale consignSale);
        /*Task UpdateConsignSaleToOnSale(Guid fashionItemId);*/
    }
}
