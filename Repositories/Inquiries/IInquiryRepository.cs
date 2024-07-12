using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessObjects.Dtos.Inquiries;
using BusinessObjects.Entities;

namespace Repositories.Inquiries
{
    public interface IInquiryRepository
    {
        Task<Inquiry> CreateInquiry(Inquiry inquiry);
    }
}
