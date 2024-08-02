using BusinessObjects.Dtos.Commons;
using BusinessObjects.Dtos.Shops;
using Repositories.Shops;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using BusinessObjects.Dtos.Feedbacks;
using BusinessObjects.Dtos.Inquiries;
using BusinessObjects.Dtos.Transactions;
using BusinessObjects.Entities;
using LinqKit;
using Microsoft.EntityFrameworkCore;
using Repositories.Inquiries;
using Repositories.Transactions;

namespace Services.Shops
{
    public class ShopService : IShopService
    {
        private readonly IShopRepository _shopRepository;
        private readonly IInquiryRepository _inquiryRepository;

        private readonly ITransactionRepository _transactionRepository;
            

        public ShopService(IShopRepository shopRepository, IInquiryRepository inquiryRepository,
            ITransactionRepository transactionRepository)
        {
            _shopRepository = shopRepository;
            _inquiryRepository = inquiryRepository;
            _transactionRepository = transactionRepository;
        }

        public async Task<Result<List<ShopDetailResponse>>> GetAllShop()
        {
            var response = new Result<List<ShopDetailResponse>>();
            var result = await _shopRepository.GetAllShop();
            if (result.Count != 0)
            {
                response.Data = result;
                response.Messages = ["Successfully"];
                response.ResultStatus = ResultStatus.Success;
                return response;
            }

            response.Messages = ["There isn't any shop available"];
            response.ResultStatus = ResultStatus.Success;
            return response;
        }

        public async Task<Result<ShopDetailResponse>> GetShopById(Guid shopid)
        {
            var response = new Result<ShopDetailResponse>();
            var result = await _shopRepository.GetShopById(shopid);
            if (result != null)
            {
                response.Data = result;
                response.Messages = ["Successfully"];
                response.ResultStatus = ResultStatus.Success;
                return response;
            }

            response.Messages = ["There isn't any shop available"];
            response.ResultStatus = ResultStatus.NotFound;
            return response;
        }

        

        public async Task<PaginationResponse<TransactionResponse>> GetOfflineTransactionsByShopId(Guid shopId,
            TransactionRequest transactionRequest)
        {
            Expression<Func<Transaction, bool>> predicate = transaction =>
                transaction.Order!.PurchaseType == PurchaseType.Offline &&
                transaction.Order!.OrderDetails.FirstOrDefault()!.FashionItem!.ShopId == shopId;

            Expression<Func<Transaction, TransactionResponse>> selector = transaction => new TransactionResponse()
            {
                TransactionId = transaction.TransactionId,
                OrderId = transaction.OrderId,
                Amount = transaction.Amount,
                CreatedDate = transaction.CreatedDate,
                CustomerName = transaction.Order!.RecipientName,
                CustomerPhone = transaction.Order.Phone
            };

            (List<TransactionResponse> Items, int Page, int PageSize, int Total) result =
                await _transactionRepository.GetTransactionsProjection<TransactionResponse>(transactionRequest.Page,
                    transactionRequest.PageSize, predicate, selector);

            return new PaginationResponse<TransactionResponse>()
            {
                Items = result.Items,
                PageNumber = result.Page,
                PageSize = result.PageSize,
                TotalCount = result.Total
            };
        }

        public Task<FeedbackResponse> CreateFeedbackForShop(Guid shopId, CreateFeedbackRequest feedbackRequest)
        {
            throw new NotImplementedException();
        }
    }
}