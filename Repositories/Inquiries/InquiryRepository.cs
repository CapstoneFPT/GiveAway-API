using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using BusinessObjects.Dtos.Inquiries;
using BusinessObjects.Entities;
using Dao;
using Microsoft.EntityFrameworkCore;

namespace Repositories.Inquiries
{
    public class InquiryRepository : IInquiryRepository
    {
        private readonly GenericDao<Inquiry> _inquiryDao;

        public InquiryRepository(GenericDao<Inquiry> inquiryDao)
        {
            _inquiryDao = inquiryDao;
        }

        public async Task<Inquiry> CreateInquiry(Inquiry inquiry)
        {
            var result = await _inquiryDao.AddAsync(inquiry);
            return result;
        }

        public async Task<(List<T> Items, int Page, int PageSize, int TotalCount)> GetInquiries<T>(
            int inquiryRequestPage, int inquiryRequestPageSize, Expression<Func<Inquiry, bool>>? predicate,
            Expression<Func<Inquiry, T>>? selector)
        {
            var query = _inquiryDao.GetQueryable();

            if (predicate != null)
            {
                query = query.Where(predicate);
            }

            var total = await query.CountAsync();

            if (inquiryRequestPage > 0 && inquiryRequestPageSize > 0)
            {
                query = query.Skip((inquiryRequestPage - 1) * inquiryRequestPageSize).Take(inquiryRequestPageSize);
            }

            List<T> items;

            if (selector != null)
            {
                items = await query.Select(selector).ToListAsync();
            }
            else
            {
                items = await query.Cast<T>().ToListAsync();
            }

            return (items, inquiryRequestPage, inquiryRequestPageSize, total);
        }
    }
}