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
        Task<bool> SendEmailRefund(RefundResponse request);
        Task<bool> SendEmailConsignSale(Guid consignSaleId);
        Task<Result<string>> SendMailForgetPassword(string email);
        Task<bool> SendEmailConsignSaleReceived(Guid consignId);
        Task<bool> SendEmailConsignSaleEndedMail(Guid consignId);
    }
}
