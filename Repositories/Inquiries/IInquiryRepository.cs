using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using BusinessObjects.Dtos.Inquiries;
using BusinessObjects.Entities;

namespace Repositories.Inquiries
{
    public interface IInquiryRepository
    {
        Task<Inquiry> CreateInquiry(Inquiry inquiry);
        Task<(List<T> Items, int Page, int PageSize, int TotalCount)> GetInquiries<T>(int inquiryRequestPage, int inquiryRequestPageSize, Expression<Func<Inquiry, bool>>? predicate, Expression<Func<Inquiry, T>>? selector);
        Task<Inquiry> ConfirmCompleted(Guid id);
    }
}
