using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessObjects.Dtos.Commons;
using BusinessObjects.Dtos.Refunds;
using BusinessObjects.Entities;

namespace Repositories.Refunds
{
    public interface IRefundRepository
    {
        Task<PaginationResponse<RefundResponse>> GetAllRefunds(RefundRequest  request);
        Task<RefundResponse> GetRefundById(Guid refundId);
        Task<RefundResponse> ApprovalRefundFromShop(Guid refundId, ApprovalRefundRequest request);
        Task<RefundResponse> ConfirmReceivedAndRefund(Guid refundId);
        Task CreateRefund(Refund refund);
    }
}
