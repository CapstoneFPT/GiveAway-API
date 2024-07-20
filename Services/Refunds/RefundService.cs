using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessObjects.Dtos.Commons;
using BusinessObjects.Dtos.Email;
using BusinessObjects.Dtos.FashionItems;
using BusinessObjects.Dtos.Refunds;
using BusinessObjects.Utils;
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

        public async Task<Result<RefundResponse>> ApprovalRefundRequestFromShop(Guid refundId, RefundStatus refundStatus)
        {
            var response = new Result<RefundResponse>();
            var refund = await _refundRepository.GetRefundById(refundId);
            
            if (refundStatus.Equals(RefundStatus.Pending) || refund.RefundStatus.Equals(RefundStatus.Pending))
            {
                throw new StatusNotAvailableException();
            }
            var data = await _refundRepository.ApprovalRefundFromShop(refundId, refundStatus);

            response.Data = data;
            response.ResultStatus = ResultStatus.Success;
            response.Messages = ["Successfully"];
            return response;
        }

        public async Task<Result<RefundResponse>> GetRefundById(Guid refundId)
        {
            var response = new Result<RefundResponse>();
            var result = await _refundRepository.GetRefundById(refundId);
            if (result is null)
            {
                response.ResultStatus = ResultStatus.NotFound;
                response.Messages = ["Can not found the refund"];
                return response;
            }

            response.Data = result;
            response.ResultStatus = ResultStatus.Success;
            response.Messages = ["Successfully"];
            return response;
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

        private async Task<bool> SendEmailRefund(Guid refundId)
        {
            SendEmailRequest content = new SendEmailRequest();
            var refund = _refundRepository.GetRefundById(refundId);
            return false;
        }
    }
}