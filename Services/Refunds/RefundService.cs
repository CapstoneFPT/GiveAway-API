using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessObjects.Dtos.Commons;
using BusinessObjects.Dtos.FashionItems;
using BusinessObjects.Dtos.Refunds;
using Org.BouncyCastle.Asn1.Ocsp;
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

        public async Task<Result<PaginationResponse<RefundResponse>>> GetRefundByShopId(Guid shopId,
            RefundRequest refundRequest)
        {
            var response = new Result<PaginationResponse<RefundResponse>>();
            var result = await _refundRepository.GetRefundsByShopId(shopId, refundRequest);
            if (result.TotalCount < 1)
            {
                response.ResultStatus = ResultStatus.Empty;
                response.Messages = ["Empty"];
                return response;
            }

            response.Data = result;
            response.ResultStatus = ResultStatus.Success;
            response.Messages = ["Results in page: " + result.PageNumber];
            return response;
        }
    }
}