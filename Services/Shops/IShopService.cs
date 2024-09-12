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
using DotNext;

namespace Services.Shops
{
    public interface IShopService
    {
        Task<BusinessObjects.Dtos.Commons.Result<List<ShopDetailResponse>>> GetAllShop();
        Task<BusinessObjects.Dtos.Commons.Result<ShopDetailResponse>> GetShopById(Guid shopid);
        
        // Task<PaginationResponse<TransactionResponse>> GetOfflineTransactionsByShopId(TransactionRequest transactionRequest);
        Task<FeedbackResponse> CreateFeedbackForShop(Guid shopId, CreateFeedbackRequest feedbackRequest);
        Task<Result<CreateShopResponse, ErrorCode>> CreateShop(CreateShopRequest request);
    }
}
