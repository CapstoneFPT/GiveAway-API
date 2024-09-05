using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessObjects.Dtos.Commons;
using BusinessObjects.Dtos.Refunds;
using Microsoft.AspNetCore.Mvc;

namespace Services.Refunds
{
    public interface IRefundService
    {
        Task<PaginationResponse<RefundResponse>> GetAllRefunds(RefundRequest refundRequest);
        Task<Result<RefundResponse>> GetRefundById(Guid refundId);
        Task<Result<RefundResponse>> ApprovalRefundRequestFromShop(Guid refundId, ApprovalRefundRequest request);
        Task<Result<RefundResponse>> ConfirmReceivedAndRefund(Guid refundId);
        Task<Result<RefundResponse>> CreateRefundByShop(Guid shopId, CreateRefundByShopRequest request);
    }
}
