using BusinessObjects.Dtos.Email;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessObjects.Dtos.Commons;
using BusinessObjects.Dtos.Refunds;
using BusinessObjects.Entities;

namespace Services.Emails
{
    public interface IEmailService
    {
        Task SendEmail(SendEmailRequest request);
        Task<Result<string>> SendMailRegister(string mail, string token);
        Task<bool> SendEmailOrder(Order order);
        Task<bool> SendEmailRefund(Guid refundId);
        Task<bool> SendEmailConsignSale(Guid consignSaleId);
        Task<Result<string>> SendMailForgetPassword(string email);
        Task<bool> SendEmailConsignSaleReceived(ConsignSale consignSale);
        Task<bool> SendEmailConsignSaleEndedMail(Guid consignId);
        Task<bool> SendEmailAuctionIsComing(Guid auctionId, Guid memberId);
        Task<bool> SendEmailAuctionWon(Guid auctionId, Bid bid);
        Task<bool> SendMailSoldItemConsign(Guid consignLineId, decimal amountConsignorReceive);
        Task<bool> SendEmailCancelOrderAndReservedItems(Order order);
        Task<bool> SendEmailConsignNegotiatePrice(ConsignSale consignSale);
    }
}
