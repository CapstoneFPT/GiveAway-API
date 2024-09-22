using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using BusinessObjects.Dtos.Commons;
using BusinessObjects.Dtos.Inquiries;
using BusinessObjects.Entities;
using DotNext;
using LinqKit;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Repositories.Inquiries;

namespace Services.Inquiries
{
    public class InquiryService : IInquiryService
    {
        private readonly IInquiryRepository _inquiryRepository;

        public InquiryService(IInquiryRepository inquiryRepository)
        {
            _inquiryRepository = inquiryRepository;
        }

        public async Task<PaginationResponse<InquiryListResponse>> GetAllInquiries(
            InquiryListRequest inquiryRequest)
        {
            Expression<Func<Inquiry, bool>> predicate = inquiry => true;
            if (!string.IsNullOrEmpty(inquiryRequest.SearchName))
            {
                predicate = predicate.And(inquiry =>
                    EF.Functions.ILike(inquiry.Member.Fullname, $"%{inquiryRequest.SearchName}%"));
            }

            if (inquiryRequest.MemberId != null)
            {
                predicate = predicate.And(inq => inq.MemberId == inquiryRequest.MemberId);
            }
            Expression<Func<Inquiry, InquiryListResponse>> selector = inquiry => new InquiryListResponse()
            {
                InquiryId = inquiry.InquiryId,
                Fullname = inquiry.Member.Fullname,
                Email = inquiry.Member.Email,
                Phone = inquiry.Member.Phone,
                Message = inquiry.Message,
                CreatedDate = inquiry.CreatedDate,
                Status = inquiry.Status
            };

            (List<InquiryListResponse> Items, int Page, int PageSize, int TotalCount) result =
                await _inquiryRepository.GetInquiries<InquiryListResponse>(inquiryRequest.Page, inquiryRequest.PageSize,
                    predicate, selector);

            return new PaginationResponse<InquiryListResponse>()
            {
                Items = result.Items,
                PageNumber = result.Page,
                PageSize = result.PageSize,
                TotalCount = result.TotalCount
            };
        }

        public async Task<Result<InquiryListResponse, ErrorCode>> ConfirmInquiryCompleted(Guid inquiryId)
        {
            var inquiry = await _inquiryRepository.ConfirmCompleted(inquiryId);
            return new InquiryListResponse()
            {
                InquiryId = inquiry.InquiryId,
                Message = inquiry.Message,
                Status = inquiry.Status
            };
        }
    }
}
