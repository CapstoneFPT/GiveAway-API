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
using Repositories.OrderDetails;
using Repositories.Orders;
using Repositories.Refunds;
using Services.Emails;

namespace Services.Refunds
{
    public class RefundService : IRefundService
    {
        private readonly IRefundRepository _refundRepository;
        private readonly IOrderRepository _orderRepository;
        private readonly IEmailService _emailService;

        public RefundService(IRefundRepository refundRepository, IOrderRepository orderRepository, IEmailService emailService)
        {
            _refundRepository = refundRepository;
            _orderRepository = orderRepository;
            _emailService = emailService;
        }

        public async Task<Result<RefundResponse>> ApprovalRefundRequestFromShop(Guid refundId, ApprovalRefundRequest request)
        {
            var response = new Result<RefundResponse>();
            var refund = await _refundRepository.GetRefundById(refundId);
            
            if (request.Status.Equals(RefundStatus.Pending) || !refund.RefundStatus.Equals(RefundStatus.Pending))
            {
                throw new StatusNotAvailableException();
            }
            var data = await _refundRepository.ApprovalRefundFromShop(refundId, request);
            await SendEmailRefund(data);
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

        public async Task<Result<PaginationResponse<RefundResponse>>> GetAllRefunds(
            RefundRequest refundRequest) 
        {
            var response = new Result<PaginationResponse<RefundResponse>>();
            var result = await _refundRepository.GetAllRefunds(refundRequest);
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

        private async Task<bool> SendEmailRefund(RefundResponse request)
        {
            SendEmailRequest content = new SendEmailRequest();
            var order = await _orderRepository.GetSingleOrder(c => c.OrderDetails.Select(c => c.OrderDetailId).Contains(request.OrderDetailId));
            if (order.Member.Email != null)
            {
                content.To = order.Member.Email;
                content.Subject = $"[GIVEAWAY] REFUND RESPONSE FROM GIVEAWAY {order.OrderCode}";
                content.Body = $@"<h2>Dear customer,<h2>
                         <h3>Thank you for purchase at GiveAway<h3><br>
                         <h3>Your item is: <h3>
                         <h4>Item Name: {request.OrderDetailsResponse.ItemName}<h4>
                         <h4>Item Price: {request.OrderDetailsResponse.UnitPrice}<h4>
                         
                         <h3>Response from shop<h3>
                         <h4>We would like to {request.RefundStatus.ToString().ToUpper()} your refund request<h4>
                         <p>Description: {request.ResponseFromShop}<p>
                         <p>Refund Percentage: {request.RefundPercentage}%
                         <p>Refund Amount: {request.RefundAmount}UP";
                await _emailService.SendEmail(content);
                return true;
            }
            return false;
        }
    }
}