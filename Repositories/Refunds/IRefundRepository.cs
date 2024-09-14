using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using BusinessObjects.Dtos.Commons;
using BusinessObjects.Dtos.Refunds;
using BusinessObjects.Entities;

namespace Repositories.Refunds
{
    public interface IRefundRepository
    {
        Task<(List<T> Items, int Page, int PageSize, int TotalCount)> GetRefundProjections<T>(
            int? page,
            int? pageSize, Expression<Func<Refund, bool>>? predicate, Expression<Func<Refund, T>>? selector);
        Task<Refund?> GetSingleRefund(Expression<Func<Refund, bool>> predicate);
        Task<RefundResponse> ApprovalRefundFromShop(Guid refundId, ApprovalRefundRequest request);
        Task<RefundResponse> ConfirmReceivedAndRefund(Guid refundId, ConfirmReceivedRequest request);
        Task CreateRefund(Refund refund);
        IQueryable<Refund> GetQueryable();
        Task UpdateRefund(Refund refund);
    }
}
