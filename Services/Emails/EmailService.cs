using BusinessObjects.Dtos.Email;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Emails
{
    public class EmailService : IEmailService
    {
        public void SendEmail(SendEmailRequest request)
        {
            var email = new MimeMessage();
            email.From.Add(MailboxAddress.Parse("alejandrin.hane@ethereal.email"));
            email.To.Add(MailboxAddress.Parse(request.To));
            email.Subject = request.Subject;
            email.Body = new TextPart(MimeKit.Text.TextFormat.Plain) { Text = request.Body };

            using var smtp = new SmtpClient();
            smtp.Connect("smtp.ethereal.emal", 587, SecureSocketOptions.StartTls);
            smtp.Authenticate("alejandrin.hane@ethereal.email", "Qh5fpDRVrwVKKMTP6W");
            smtp.Send(email);
            smtp.Disconnect(true);
        }
    }
}
