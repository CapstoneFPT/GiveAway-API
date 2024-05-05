using BusinessObjects.Dtos.Email;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Emails
{
    public interface IEmailService
    {
        void SendEmail(SendEmailRequest request);
    }
}
