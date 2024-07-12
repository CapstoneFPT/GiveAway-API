using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessObjects.Dtos.Commons;
using BusinessObjects.Dtos.Refunds;

namespace Services.Refunds
{
    public interface IRefundService
    {
        Task<Result<PaginationResponse<RefundResponse>>> GetRefundByShopId(Guid shopId, RefundRequest refundRequest);
    }
}
