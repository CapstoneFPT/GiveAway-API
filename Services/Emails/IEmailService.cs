using BusinessObjects.Dtos.Email;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessObjects.Dtos.Commons;

namespace Services.Emails
{
    public interface IEmailService
    {
        Task SendEmail(SendEmailRequest request);
        Task<Result<string>> SendMailRegister(string mail, string token);
    }
}
