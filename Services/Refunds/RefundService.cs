using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessObjects.Dtos.Commons;
using BusinessObjects.Dtos.Refunds;
using Repositories.Refunds;

namespace Services.Refunds
{
    public class RefundService : IRefundService
    {
        private readonly IRefundRepository _refundRepository;

        public RefundService(IRefundRepository refundRepository)
        {
            _refundRepository = refundRepository;
        }

        public Task<Result<PaginationResponse<RefundResponse>>> GetRefundByShopId(Guid shopId,
            RefundRequest refundRequest)
        {
            throw new NotImplementedException();
        }
    }
}