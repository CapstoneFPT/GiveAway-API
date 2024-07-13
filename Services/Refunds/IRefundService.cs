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
        Task<Result<PaginationResponse<RefundResponse>>> GetRefundByShopId(Guid shopId, RefundRequest refundRequest);
        Task<Result<RefundResponse>> GetRefundById(Guid refundId);
        Task<Result<RefundResponse>> ApprovalRefundRequestFromShop(Guid refundId, RefundStatus refundStatus);
    }
}
