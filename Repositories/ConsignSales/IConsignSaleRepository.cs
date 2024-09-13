using BusinessObjects.Dtos.Commons;
using BusinessObjects.Dtos.ConsignSales;
using BusinessObjects.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using DotNext;

namespace Repositories.ConsignSales
{
    public interface IConsignSaleRepository
    {
        Task<PaginationResponse<ConsignSaleDetailedResponse>> GetAllConsignSale(Guid accountId,
            ConsignSaleRequest request);

        Task<ConsignSaleDetailedResponse?> GetConsignSaleById(Guid consignId);
        Task<ConsignSaleDetailedResponse> CreateConsignSale(Guid accountId, CreateConsignSaleRequest request);
        Task<ConsignSaleDetailedResponse> ApprovalConsignSale(Guid consignId, ApproveConsignSaleRequest request);
        Task<List<ConsignSale>> GetAllConsignPendingByAccountId(Guid accountId, bool isTracking = false);
        Task<ConsignSaleDetailedResponse> ConfirmReceivedFromShop(Guid consignId);
        Task<ConsignSale> CreateConsignSaleByShop(ConsignSale consignSale);

        Task<PaginationResponse<ConsignSaleDetailedResponse>> GetAllConsignSaleByShopId(
            ConsignSaleRequestForShop request);

        Task<ConsignSale?> GetSingleConsignSale(Expression<Func<ConsignSale, bool>> predicate);

        Task UpdateConsignSale(ConsignSale consignSale);

        /*Task UpdateConsignSaleToOnSale(Guid fashionItemId);*/
        Task<(List<T> Items, int Page, int PageSize, int TotalCount)> GetConsignSalesProjections<T>(
            Expression<Func<ConsignSale, bool>>? predicate, Expression<Func<ConsignSale, T>>? selector,
            int? requestPage, int? requestPageSize);

        IQueryable<ConsignSale> GetQueryable();
        Task<string> GenerateUniqueString();
    }
}