using System.Linq.Expressions;
using BusinessObjects.Dtos.ConsignSaleDetails;
using BusinessObjects.Entities;

namespace Repositories.ConsignSaleDetails;

public interface IConsignSaleDetailRepository
{
    Task<List<ConsignSaleDetailResponse>> GetConsignSaleDetailsByConsignSaleId(Guid consignSaleId);
    Task<ConsignSaleDetail?> GetSingleConsignSaleDetail(Expression<Func<ConsignSaleDetail, bool>> predicate);
    Task UpdateConsignSaleDetail(ConsignSaleDetail consignSaleDetail);
    IQueryable<ConsignSaleDetail> GetQueryable();
}