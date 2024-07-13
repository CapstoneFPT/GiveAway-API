using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessObjects.Dtos.Commons;
using BusinessObjects.Dtos.Refunds;

namespace Repositories.Refunds
{
    public interface IRefundRepository
    {
        Task<PaginationResponse<RefundResponse>> GetRefundsByShopId(Guid shopId, RefundRequest  request);
        Task<RefundResponse> GetRefundById(Guid refundId);
        Task<RefundResponse> ApprovalRefundFromShop(Guid refundId, RefundStatus refundStatus);
    }
}
