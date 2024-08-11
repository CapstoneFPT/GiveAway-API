using BusinessObjects.Dtos.Commons;
using BusinessObjects.Dtos.Shops;
using Repositories.Shops;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using BusinessObjects.Dtos.Feedbacks;
using BusinessObjects.Dtos.Inquiries;
using BusinessObjects.Dtos.Transactions;
using BusinessObjects.Entities;
using BusinessObjects.Utils;
using DotNext;
using LinqKit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NetTopologySuite.Geometries;
using Repositories.Inquiries;
using Repositories.Transactions;
using Services.GiaoHangNhanh;

namespace Services.Shops
{
    public class ShopService : IShopService
    {
        private readonly IShopRepository _shopRepository;
        private readonly IInquiryRepository _inquiryRepository;
        private readonly IGiaoHangNhanhService _giaoHangNhanhService;
        private readonly ITransactionRepository _transactionRepository;
        private readonly ILogger<ShopService> _logger;

        public ShopService(IShopRepository shopRepository, IInquiryRepository inquiryRepository,
            ITransactionRepository transactionRepository, IGiaoHangNhanhService giaoHangNhanhService,
            ILogger<ShopService> logger)
        {
            _shopRepository = shopRepository;
            _inquiryRepository = inquiryRepository;
            _transactionRepository = transactionRepository;
            _giaoHangNhanhService = giaoHangNhanhService;
            _logger = logger;
        }

        public async Task<BusinessObjects.Dtos.Commons.Result<List<ShopDetailResponse>>> GetAllShop()
        {
            var response = new BusinessObjects.Dtos.Commons.Result<List<ShopDetailResponse>>();
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

        public async Task<BusinessObjects.Dtos.Commons.Result<ShopDetailResponse>> GetShopById(Guid shopid)
        {
            var response = new BusinessObjects.Dtos.Commons.Result<ShopDetailResponse>();
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
                // transaction.Order!.PurchaseType == PurchaseType.Offline &&
                // transaction.Order!.OrderDetails.FirstOrDefault()!.IndividualFashionItem!.ShopId == shopId;
                true;

            Expression<Func<Transaction, TransactionResponse>> selector = transaction => new TransactionResponse()
            {
                TransactionId = transaction.TransactionId,
                OrderId = transaction.OrderId,
                Amount = transaction.Amount,
                CreatedDate = transaction.CreatedDate,
                CustomerName = transaction.Order!.RecipientName,
                CustomerPhone = transaction.Order.Phone
            };
            Expression<Func<Transaction, DateTime>> orderBy = transaction => transaction.CreatedDate;
            (List<TransactionResponse> Items, int Page, int PageSize, int Total) result =
                await _transactionRepository.GetTransactionsProjection<TransactionResponse>(transactionRequest.Page,
                    transactionRequest.PageSize, predicate, orderBy, selector);

            return new PaginationResponse<TransactionResponse>()
            {
                Items = result.Items,
                PageNumber = result.Page,
                PageSize = result.PageSize,
                TotalCount = result.Total
            };
        }

        public async Task<DotNext.Result<CreateShopResponse, ErrorCode>> CreateShop(CreateShopRequest request)
        {
            var ghnRequest = new GHNShopCreateRequest()
            {
                Name = request.Name,
                Phone = request.Phone,
                Address = request.Address,
                WardCode = request.WardCode,
                DistrictId = request.DistrictId
            };

            var ghnResult = await _giaoHangNhanhService.CreateShop(ghnRequest);

            if (!ghnResult.IsSuccessful)
            {
                return new Result<CreateShopResponse, ErrorCode>(ghnResult.Error);
            }

            var ghnShops = await _giaoHangNhanhService
                .GetShops();

            if (!ghnShops.IsSuccessful)
            {
                return new Result<CreateShopResponse, ErrorCode>(ghnShops.Error);
            }

            var ghnShop = ghnShops
                .Value
                .Data!
                .Shops
                .Find(x => x.Id == ghnResult.Value.Data!.ShopId);
            try
            {
                if (!int.TryParse(request.WardCode, out int wardCode))
                    return new Result<CreateShopResponse, ErrorCode>(ErrorCode.InvalidInput);
                
                
                var shop = new Shop()
                {
                    Phone = request.Phone,
                    Address = await _giaoHangNhanhService.BuildAddress(request.ProvinceId, request.DistrictId,
                        wardCode,
                        request.Address),
                    CreatedDate = DateTime.UtcNow,
                    Location = new Point(10.835655304770324,
                        106.80765455517754), //Who cares about the location anyway,
                    ShopCode = Guid.NewGuid().ToString(),
                    GhnShopId = ghnResult.Value.Data!.ShopId,
                    GhnWardCode = ghnShop!.WardCode,
                    GhnDistrictId = ghnShop.DistrictId,
                    StaffId = request.StaffId
                };

                var createdShop = await _shopRepository.CreateShop(shop);
                return new Result<CreateShopResponse, ErrorCode>(new CreateShopResponse()
                {
                });

            }
            catch (DbCustomException e)
            {
                _logger.LogError(e, "Error while creating shop");
                return new Result<CreateShopResponse, ErrorCode>(ErrorCode.ServerError);
            }
        }

    

        public Task<FeedbackResponse> CreateFeedbackForShop(Guid shopId, CreateFeedbackRequest feedbackRequest)
        {
            throw new NotImplementedException();
        }
    }

    public class CreateShopRequest
    {
        [Required] public string Name { get; set; }
        [Required] [Phone] public string Phone { get; set; }
        [Required] public string Address { get; set; }
        [Required] public string WardCode { get; set; }
        [Required] public int DistrictId { get; set; }
        [Required] public int ProvinceId { get; set; }
        [Required] public Guid StaffId { get; set; }
    }

    public class CreateShopResponse
    {
    }
}