using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessObjects.Dtos.Inquiries;
using BusinessObjects.Entities;
using Dao;

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
    }
}
