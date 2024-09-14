using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessObjects.Dtos.Commons;
using BusinessObjects.Dtos.Refunds;
using DotNext;
using Microsoft.AspNetCore.Mvc;

namespace Services.Refunds
{
    public interface IRefundService
    {
        Task<PaginationResponse<RefundResponse>> GetAllRefunds(RefundRequest refundRequest);
        Task<Result<RefundResponse, ErrorCode>> GetRefundById(Guid refundId);
        Task<BusinessObjects.Dtos.Commons.Result<RefundResponse>> ApprovalRefundRequestFromShop(Guid refundId, ApprovalRefundRequest request);
        Task<BusinessObjects.Dtos.Commons.Result<RefundResponse>> ConfirmReceivedAndRefund(Guid refundId, ConfirmReceivedRequest request);
        Task<Result<RefundResponse, ErrorCode>> CreateRefundByShop(Guid shopId, CreateRefundByShopRequest request);
        Task<Result<RefundResponse,ErrorCode>> CancelRefund(Guid refundId);
        Task<Result<RefundResponse,ErrorCode>> UpdateRefund(Guid refundId, UpdateRefundRequest request);
    }
}
