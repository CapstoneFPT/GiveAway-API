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
using Repositories.Accounts;

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
            _templateDirectory = Path.Combine("/app/" , configuration["EmailTemplateDirectory"]);
        }
        public string GetEmailTemplate(string templateName)
        {
            var templatePath = Path.Combine(_templateDirectory, templateName);
            return File.ReadAllText(templatePath);
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

            var template = GetEmailTemplate("VerifyAccountMail.html");
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
    }
}
