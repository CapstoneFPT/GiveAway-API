using BusinessObjects.Dtos.Commons;
using BusinessObjects.Dtos.Shops;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessObjects.Dtos.Feedbacks;
using BusinessObjects.Dtos.Inquiries;
using BusinessObjects.Dtos.Transactions;

namespace Services.Shops
{
    public interface IShopService
    {
        Task<Result<List<ShopDetailResponse>>> GetAllShop();
        Task<Result<ShopDetailResponse>> GetShopById(Guid shopid);
        Task<PaginationResponse<InquiryListResponse>> GetInquiriesByShopId(Guid shopId, InquiryListRequest inquiryRequest);
        Task<PaginationResponse<TransactionResponse>> GetOfflineTransactionsByShopId(Guid shopId, TransactionRequest transactionRequest);
        Task<FeedbackResponse> CreateFeedbackForShop(Guid shopId, CreateFeedbackRequest feedbackRequest);
    }
}
