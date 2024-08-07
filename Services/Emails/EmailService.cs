using BusinessObjects.Dtos.Email;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using MimeKit.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessObjects.Dtos.Commons;
using BusinessObjects.Dtos.Refunds;
using Repositories.Accounts;
using BusinessObjects.Entities;
using Dao;
using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Bcpg.Attr;
using Repositories.ConsignSales;
using Repositories.Orders;

namespace Services.Emails
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly IAccountRepository _accountRepository;
        private readonly IOrderRepository _orderRepository;
        private readonly IConsignSaleRepository _consignSaleRepository;
        
        public EmailService(IConfiguration configuration, IAccountRepository accountRepository, IOrderRepository orderRepository,
            IConsignSaleRepository consignSaleRepository)
        {
            _configuration = configuration;
            _accountRepository = accountRepository;
            _orderRepository = orderRepository;
            _consignSaleRepository = consignSaleRepository;
        }
        public string GetEmailTemplate(string templateName)
        {
            string pathTon = Path.Combine("D:\\Captstone\\GiveAway-API\\Services\\MailTemplate\\", $"{templateName}.html");
            // string pathLocal = Path.Combine("C:\\FPT_University_FULL\\CAPSTONE_API\\Services\\MailTemplate\\", $"{templateName}.html");*/
            string path = Path.Combine(_configuration.GetSection("EmailTemplateDirectory").Value, $"{templateName}.html");
            var template = File.ReadAllText(pathTon, Encoding.UTF8);
            template = template.Replace("[path]", _configuration.GetSection("RedirectDirectory").Value);
            return template;
        }

        public async Task SendEmail(SendEmailRequest request)
        {
            var email = new MimeMessage();
            email.From.Add(MailboxAddress.Parse(_configuration.GetSection("MailSettings:Mail").Value));
            email.To.Add(MailboxAddress.Parse(request.To));
            email.Subject = request.Subject;
            email.Body = new TextPart(TextFormat.Html) { Text = request.Body };


            // dùng SmtpClient của MailKit
            using var smtp = new SmtpClient();
            await smtp.ConnectAsync(_configuration.GetSection("MailSettings:Host").Value, 587, SecureSocketOptions.Auto);
            await smtp.AuthenticateAsync(_configuration.GetSection("MailSettings:Mail").Value, _configuration.GetSection("MailSettings:Password").Value);
            await smtp.SendAsync(email);
            await smtp.DisconnectAsync(true);
        }
        public async Task<Result<string>> SendMailRegister(string email, string token)
        {
            var response = new Result<string>();
            var user = await _accountRepository.FindUserByEmail(email);
            string appDomain = _configuration.GetSection("MailSettings:AppDomain").Value;
            string confirmationLink = _configuration.GetSection("MailSettings:EmailConfirmation").Value;
            string formattedLink = string.Format(appDomain + confirmationLink, user.AccountId, token);

            var template = GetEmailTemplate("VerifyAccountMail");
            template = template.Replace($"[link]", formattedLink);

                SendEmailRequest content = new SendEmailRequest
            {
                To = email,
                Subject = "[GIVEAWAY] Verify Account",
                Body = template,
            };
            await SendEmail(content);
            response.Messages = ["Register successfully! Please check your email for verification in 3 minutes"];
            response.ResultStatus = ResultStatus.Success;
            return response;
        }
        public async Task<Result<string>> SendEmailOrder(Order order)
        {
            var response = new Result<string>();
            SendEmailRequest content = new SendEmailRequest();
            if (order.MemberId != null)
            {
                var member = await _accountRepository.GetAccountById(order.MemberId.Value);
                content.To = member.Email;
            }
            else
            {
                content.To = order.Email;
            }

            var template = GetEmailTemplate("OrderMail");
            template = template.Replace($"[Order Code]", order.OrderCode);
            template = template.Replace($"[Quantity]", order.OrderDetails.Count().ToString());
            template = template.Replace($"[Payment Method]", order.PaymentMethod.ToString());
            template = template.Replace($"[Payment Date]", order.PaymentDate.GetValueOrDefault().ToString("G"));
            template = template.Replace($"[Total Price]", order.TotalPrice.ToString("N0"));
            template = template.Replace($"[Recipient Name]", order.RecipientName);
            template = template.Replace($"[Phone Number]", order.Phone);
            template = template.Replace($"[Email]", order.Email);
            template = template.Replace($"[Address]", order.Address);
            /*template = template.Replace("[path]", _configuration.GetSection("RedirectDirectory").Value);*/
            content.Subject = $"[GIVEAWAY] ORDER INVOICE FROM GIVEAWAY";
            content.Body = template;
            await SendEmail(content);
            response.Messages = ["The invoice has been send to customer mail"];
            response.ResultStatus = ResultStatus.Success;
            return response;
        }
        public async Task<bool> SendEmailRefund(RefundResponse request)
        {
            SendEmailRequest content = new SendEmailRequest();
            var order = await _orderRepository.GetSingleOrder(c => c.OrderDetails.Select(c => c.OrderDetailId).Contains(request.OrderDetailId));
            var template = GetEmailTemplate("RefundMail");
            template = template.Replace("[Order Code]", order.OrderCode);   
            template = template.Replace("[Status]", request.RefundStatus.ToString());   
            template = template.Replace("[Product Name]", request.OrderDetailsResponse.ItemName);
            template = template.Replace("[Created Date]", request.CreatedDate.ToString("G"));
            template = template.Replace("[Refund Percent]", request.RefundPercentage.Value.ToString());
            template = template.Replace("[Refund Amount]", request.RefundAmount.Value.ToString());
            template = template.Replace("[Customer Name]", request.CustomerName);
            template = template.Replace("[Phone Number]", request.CustomerPhone);
            template = template.Replace("[Email]", request.CustomerEmail);
            template = template.Replace("[Description]", request.Description);
            template = template.Replace("[Response]", request.ResponseFromShop);
            if (order.MemberId != null)
            {
                var member = await GenericDao<Account>.Instance.GetQueryable().Where(c => c.AccountId == order.MemberId)
                    .FirstOrDefaultAsync();
                content.To = member.Email;
                content.Subject = $"[GIVEAWAY] REFUND RESPONSE FROM GIVEAWAY";
                content.Body = template;
                         
                await SendEmail(content);
                return true;
            }
            return false;
        }
        public async Task<bool> SendEmailConsignSale(Guid consignSaleId)
        {
            var consignSale = await _consignSaleRepository.GetConsignSaleById(consignSaleId);
            SendEmailRequest content = new SendEmailRequest();
            if (consignSale.MemberId != null)
            {
                var member = await _accountRepository.GetAccountById(consignSale.MemberId.Value);
                content.To = member.Email;

                var template = GetEmailTemplate("ConsignSaleMail");
                template = template.Replace("[ConsignSale Code]", consignSale.ConsignSaleCode);
                template = template.Replace("[Type]", consignSale.Type.ToString());
                template = template.Replace("[Created Date]", consignSale.CreatedDate.ToString("G"));
                template = template.Replace("[Customer Name]", consignSale.Consginer);
                template = template.Replace("[Phone Number]", consignSale.Phone);
                template = template.Replace("[Email]", consignSale.Email);
                template = template.Replace("[Address]", consignSale.Address);
                if (consignSale.Status.Equals(ConsignSaleStatus.AwaitDelivery))
                {
                    template = template.Replace("[Status]", "Approved");
                    template = template.Replace("[Response]", "Please deliver your products to our shop as soon as possible");
                    template = template.Replace("[ConsignSale Duration]", "60 Days");
                }
                else
                {
                    template = template.Replace("[Status]", "Rejected");
                    template = template.Replace("[Response]", "Your products is kindly not suitable to our shop. We are so apologize");
                    template = template.Replace("[ConsignSale Duration]", "0 Day");
                }
                
                content.Subject = $"[GIVEAWAY] CONSIGNSALE INVOICE FROM GIVEAWAY";
                content.Body = template;

                await SendEmail(content);
                return true;
            }
            return false;
        }
        public async Task<Result<string>> SendMailForgetPassword(string email)
        {
            var response = new Result<string>();
            var account = await _accountRepository.FindUserByEmail(email);
            var user = await _accountRepository.ResetPasswordToken(account);
            response.Data = user.PasswordResetToken;
            var template = GetEmailTemplate("ForgotPasswordMail");
            template = template.Replace("[Token]", response.Data);
            SendEmailRequest content = new SendEmailRequest
            {
                To = $"{account.Email}",
                Subject = "[GIVEAWAY] RESET PASSWORD",
                Body = template
            };
            await SendEmail(content);
            response.Messages = ["Please check your mail to get the token to reset password"];
            response.ResultStatus = ResultStatus.Success;

            return response;
        }

        public async Task<bool> SendEmailConsignSaleReceived(Guid consignId)
        {
            var consignSale = await _consignSaleRepository.GetConsignSaleById(consignId);
            SendEmailRequest content = new SendEmailRequest();
            if (consignSale.MemberId != null)
            {
                var member = await _accountRepository.GetAccountById(consignSale.MemberId.Value);
                content.To = member.Email;

                var template = GetEmailTemplate("ConsignSaleReceivedMail");
                template = template.Replace("[ConsignSale Code]", consignSale.ConsignSaleCode);
                template = template.Replace("[Type]", consignSale.Type.ToString());
                template = template.Replace("[Start Date]", consignSale.StartDate.GetValueOrDefault().ToString("G"));
                template = template.Replace("[Customer Name]", consignSale.Consginer);
                template = template.Replace("[Phone Number]", consignSale.Phone);
                template = template.Replace("[Email]", consignSale.Email);
                template = template.Replace("[Address]", consignSale.Address);
                template = template.Replace("[Response]", "Thank you for trusting and using the consignment service at Give Away store.");
                template = template.Replace("[End Date]", consignSale.EndDate.GetValueOrDefault().ToString("G"));
                
                content.Subject = $"[GIVEAWAY] RECEIVED CONSIGNSALE FROM GIVEAWAY";
                content.Body = template;

                await SendEmail(content);
                return true;
            }
            return false;
        }

        public async Task<bool> SendEmailConsignSaleEndedMail(Guid consignId)
        {
            var consignSale = await _consignSaleRepository.GetConsignSaleById(consignId);
            SendEmailRequest content = new SendEmailRequest();
            if (consignSale.MemberId != null)
            {
                var member = await _accountRepository.GetAccountById(consignSale.MemberId.Value);
                content.To = member.Email;

                var template = GetEmailTemplate("ConsignSaleEndedMail");
                template = template.Replace("[ConsignSale Code]", consignSale.ConsignSaleCode);
                template = template.Replace("[Type]", consignSale.Type.ToString());
                template = template.Replace("[Total Price]", consignSale.TotalPrice.ToString("N0"));
                template = template.Replace("[Sold Price]", consignSale.SoldPrice.ToString("N0"));
                template = template.Replace("[Amount Receive]", consignSale.MemberReceivedAmount.ToString("N0"));
                template = template.Replace("[Phone Number]", consignSale.Phone);
                template = template.Replace("[Email]", consignSale.Email);
                template = template.Replace("[Address]", consignSale.Address);
                template = template.Replace("[Response]", "Thank you for trusting and using the consignment service at Give Away store.");
                if (consignSale.SoldPrice < 1000000)
                {
                    template = template.Replace("[Consignment Fee]", "26%");
                }
                else if(consignSale.SoldPrice >= 1000000 && consignSale.SoldPrice <= 10000000)
                {
                    template = template.Replace("[Consignment Fee]", "23%");
                }
                else
                {
                    template = template.Replace("[Consignment Fee]", "20%");
                }
                content.Subject = $"[GIVEAWAY] CONSIGNMENT ENDED FROM GIVEAWAY";
                content.Body = template;

                await SendEmail(content);
                return true;
            }
            return false;
        }
    }
}
