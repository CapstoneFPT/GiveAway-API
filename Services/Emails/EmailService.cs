﻿using BusinessObjects.Dtos.Email;
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
using Repositories.Accounts;
using BusinessObjects.Entities;

namespace Services.Emails
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly IAccountRepository _accountRepository;
        private readonly string _templateDirectory;
        public EmailService(IConfiguration configuration, IAccountRepository accountRepository)
        {
            _configuration = configuration;
            _accountRepository = accountRepository;
            _templateDirectory = Path.Combine(AppContext.BaseDirectory, configuration["EmailTemplateDirectory"]);
        }
        public string GetEmailTemplate(string templateName)
        {
            string pathLocal = Path.Combine("C:\\FPT_University_FULL\\CAPSTONE_API\\Services\\MailTemplate\\", $"{templateName}.html");
            string path = Path.Combine("Services/MailTemplate/", $"{templateName}.html");
            return File.ReadAllText(pathLocal, Encoding.UTF8);
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
            template = template.Replace($"[Payment Date]", order.PaymentDate.ToString());
            template = template.Replace($"[Total Price]", order.PaymentDate.ToString());
            template = template.Replace($"[Recipient Name]", order.RecipientName);
            template = template.Replace($"[Phone Number]", order.Phone);
            template = template.Replace($"[Email]", order.Email);
            template = template.Replace($"[Address]", order.Address);
            content.Subject = $"[GIVEAWAY] ORDER INVOICE FROM GIVEAWAY";
            content.Body = template;
            await SendEmail(content);
            response.Messages = ["The invoice has been send to customer mail"];
            response.ResultStatus = ResultStatus.Success;
            return response;
        }
    }
}
