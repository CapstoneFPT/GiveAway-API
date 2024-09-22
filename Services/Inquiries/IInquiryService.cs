using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessObjects.Dtos.Commons;
using BusinessObjects.Dtos.Inquiries;

namespace Services.Inquiries
{
    public interface IInquiryService
    {
        Task<PaginationResponse<InquiryListResponse>> GetAllInquiries(InquiryListRequest inquiryRequest);
        Task<DotNext.Result<InquiryListResponse, ErrorCode>> ConfirmInquiryCompleted(Guid inquiryId);
    }
}
