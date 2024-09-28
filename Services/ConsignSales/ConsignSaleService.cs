using System.Linq.Expressions;
using System.Text;
using AutoMapper;
using BusinessObjects.Dtos.Commons;
using BusinessObjects.Dtos.ConsignSaleLineItems;
using BusinessObjects.Dtos.ConsignSales;
using BusinessObjects.Dtos.Email;
using BusinessObjects.Dtos.FashionItems;
using BusinessObjects.Dtos.OrderLineItems;
using BusinessObjects.Dtos.Orders;
using BusinessObjects.Dtos.Shops;
using BusinessObjects.Entities;
using BusinessObjects.Utils;
using DotNext;
using IronPdf.Rendering;
using LinqKit;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using Org.BouncyCastle.Asn1.Ocsp;
using Quartz;
using Repositories.Accounts;
using Repositories.ConsignSaleLineItems;
using Repositories.ConsignSales;
using Repositories.FashionItems;
using Repositories.Images;
using Repositories.Orders;
using Repositories.Schedules;
using Repositories.Shops;
using Repositories.Transactions;
using Repositories.Utils;
using Services.Emails;

namespace Services.ConsignSales
{
    public class ConsignSaleService : IConsignSaleService
    {
        private readonly IConsignSaleRepository _consignSaleRepository;
        private readonly IAccountRepository _accountRepository;
        private readonly IConsignSaleLineItemRepository _consignSaleLineItemRepository;
        private readonly IOrderRepository _orderRepository;
        private readonly IEmailService _emailService;
        private readonly IMapper _mapper;
        private readonly ISchedulerFactory _schedulerFactory;
        private readonly IFashionItemRepository _fashionItemRepository;
        private readonly IImageRepository _imageRepository;
        private readonly ILogger<ConsignSaleService> _logger;
        private readonly ITransactionRepository _transactionRepository;
        private readonly IShopRepository _shopRepository;

        public ConsignSaleService(IConsignSaleRepository consignSaleRepository, IAccountRepository accountRepository,
            IConsignSaleLineItemRepository consignSaleLineItemRepository
            , IOrderRepository orderRepository, IEmailService emailService, IMapper mapper,
            ISchedulerFactory schedulerFactory, IFashionItemRepository fashionItemRepository,
            IImageRepository imageRepository, ILogger<ConsignSaleService> logger, ITransactionRepository transactionRepository, IShopRepository shopRepository)
        {
            _consignSaleRepository = consignSaleRepository;
            _accountRepository = accountRepository;
            _consignSaleLineItemRepository = consignSaleLineItemRepository;
            _orderRepository = orderRepository;
            _emailService = emailService;
            _mapper = mapper;
            _schedulerFactory = schedulerFactory;
            _fashionItemRepository = fashionItemRepository;
            _imageRepository = imageRepository;
            _logger = logger;
            _transactionRepository = transactionRepository;
            _shopRepository = shopRepository;
        }

        public async Task<BusinessObjects.Dtos.Commons.Result<ConsignSaleDetailedResponse>> ApprovalConsignSale(
            Guid consignId,
            ApproveConsignSaleRequest request)
        {
            var response = new BusinessObjects.Dtos.Commons.Result<ConsignSaleDetailedResponse>();
            var consign = await _consignSaleRepository.GetConsignSaleById(consignId);
            if (consign == null)
            {
                throw new ConsignSaleNotFoundException();
            }

            if (!consign.Status.Equals(ConsignSaleStatus.Pending))
            {
                throw new StatusNotAvailableWithMessageException("This consign is not pending for approval");
            }

            if (!request.Status.Equals(ConsignSaleStatus.AwaitDelivery) &&
                !request.Status.Equals(ConsignSaleStatus.Rejected))
            {
                throw new StatusNotAvailableException();
            }

            response.Data = await _consignSaleRepository.ApprovalConsignSale(consignId, request);
            var mailResult = await _emailService.SendEmailConsignSale(consignId);
            response.Messages = ["Approval successfully"];
            if (mailResult is false)
            {
                response.Messages = new[] { "Approve successfully but mail fail" };
            }
            response.ResultStatus = ResultStatus.Success;
            return response;
        }

        public async Task<BusinessObjects.Dtos.Commons.Result<ConsignSaleDetailedResponse>> ConfirmReceivedFromShop(
            Guid consignId)
        {
            var response = new BusinessObjects.Dtos.Commons.Result<ConsignSaleDetailedResponse>();
            var consign = await _consignSaleRepository.GetSingleConsignSale(c => c.ConsignSaleId == consignId);
            if (consign == null)
            {
                throw new ConsignSaleNotFoundException();
            }

            if (!consign.Status.Equals(ConsignSaleStatus.AwaitDelivery))
            {
                throw new StatusNotAvailableWithMessageException("This consign is not awaiting for delivery");
            }

            var result = await _consignSaleRepository.ConfirmReceivedFromShop(consignId);
            // await ScheduleConsignEnding(result);
            await _emailService.SendEmailConsignSaleReceived(consign);
            response.Data = result;
            response.Messages = ["Confirm received successfully"];
            response.ResultStatus = ResultStatus.Success;
            return response;
        }

        private async Task ScheduleConsignEnding(ConsignSale consign)
        {
            var schedule = await _schedulerFactory.GetScheduler();
            var jobDataMap = new JobDataMap()
            {
                { "ConsignId", consign.ConsignSaleId }
            };
            var endJob = JobBuilder.Create<ConsignEndingJob>()
                .WithIdentity($"EndConsign_{consign.ConsignSaleId}")
                .SetJobData(jobDataMap)
                .Build();
            var endTrigger = TriggerBuilder.Create()
                .WithIdentity($"EndConsignTrigger_{consign.ConsignSaleId}")
                .StartAt(new DateTimeOffset(consign.EndDate!.Value))
                .Build();
            await schedule.ScheduleJob(endJob, endTrigger);
        }

        public async Task<BusinessObjects.Dtos.Commons.Result<ConsignSaleDetailedResponse>> CreateConsignSale(
            Guid accountId,
            CreateConsignSaleRequest request)
        {
            var response = new BusinessObjects.Dtos.Commons.Result<ConsignSaleDetailedResponse>();
            //check account co' active hay ko
            var account = await _accountRepository.GetAccountById(accountId);
            if (account == null || account.Status.Equals(AccountStatus.Inactive) ||
                account.Status.Equals(AccountStatus.NotVerified))
            {
                response.Messages = ["This account is not available to consign"];
                response.ResultStatus = ResultStatus.Error;
                return response;
            }

            var consign = await _consignSaleRepository.CreateConsignSale(accountId, request);
            if (consign == null)
            {
                response.Messages = ["There is an error. Can not find consign"];
                response.ResultStatus = ResultStatus.Error;
                return response;
            }

            response.Data = consign;
            response.ResultStatus = ResultStatus.Success;
            response.Messages = ["Create successfully"];
            return response;
        }

        public async Task<BusinessObjects.Dtos.Commons.Result<ConsignSaleDetailedResponse>> CreateConsignSaleByShop(
            Guid shopId,
            CreateConsignSaleByShopRequest request)
        {
            var response = new BusinessObjects.Dtos.Commons.Result<ConsignSaleDetailedResponse>();
            if (request.ConsignDetailRequests.Count == 0)
            {
                throw new MissingFeatureException("You must have at least one product");
            }

            var member = await _accountRepository.FindUserByPhone(request.Phone);
            var listMasterId = request.ConsignDetailRequests.Select(c => c.MasterItemId);
            var listMasterItem = await _fashionItemRepository.GetMasterQueryable()
                .Where(c => listMasterId.Contains(c.MasterItemId)).ToListAsync();
            if (listMasterItem.Any(c => c.IsConsignment == false))
            {
                throw new MasterItemNotAvailableException("There are master that not allow to choose");
            }

            var newConsign = new ConsignSale()
            {
                Type = request.Type,
                CreatedDate = DateTime.UtcNow,
                Status = ConsignSaleStatus.OnSale,
                ConsignSaleMethod = ConsignSaleMethod.Offline,
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddDays(60),
                TotalPrice = request.ConsignDetailRequests.Sum(c => c.ExpectedPrice),
                SoldPrice = 0,
                ShopId = shopId,
                ConsignorReceivedAmount = 0,
                ConsignSaleCode = await _consignSaleRepository.GenerateUniqueString(),
                ConsignorName = request.ConsignorName,
                Address = request.Address,
                Email = request.Email,
                Phone = request.Phone,
                MemberId = member?.AccountId,
            };

            var newConsignLineItem = new List<ConsignSaleLineItem>();

            foreach (var consignDetailRequest in request.ConsignDetailRequests)
            {
                var masterItem = listMasterItem
                    .FirstOrDefault(c => c.MasterItemId == consignDetailRequest.MasterItemId);
                if (masterItem is null)
                {
                    throw new MasterItemNotAvailableException("Can not found master item in list master item id" + consignDetailRequest.MasterItemId);
                }
                var consignLineItem = new ConsignSaleLineItem()
                {
                    ConfirmedPrice = consignDetailRequest.ExpectedPrice,
                    DealPrice = consignDetailRequest.ExpectedPrice,
                    ConsignSaleId = newConsign.ConsignSaleId,
                    Note = consignDetailRequest.Note,
                    CreatedDate = DateTime.UtcNow,
                    Size = consignDetailRequest.Size,
                    Gender = masterItem.Gender,
                    ProductName = masterItem.Name,
                    Condition = consignDetailRequest.Condition,
                    Color = consignDetailRequest.Color,
                    Status = ConsignSaleLineItemStatus.OnSale,
                    Brand = masterItem.Brand,
                    IsApproved = true,
                    ExpectedPrice = consignDetailRequest.ExpectedPrice
                };
                consignLineItem.Images = consignDetailRequest.ImageUrls.Select(url => new Image()
                {
                    ConsignLineItemId = consignLineItem.ConsignSaleId,
                    Url = url,
                    CreatedDate = DateTime.UtcNow
                }).ToList();

                var individualItem = new IndividualFashionItem()
                {
                    Note = consignLineItem.Note,
                    CreatedDate = DateTime.UtcNow,
                    MasterItemId = consignDetailRequest.MasterItemId,
                    ItemCode = await _fashionItemRepository.GenerateIndividualItemCode(masterItem.MasterItemCode),
                    Status = FashionItemStatus.Available,
                    ConsignSaleLineItemId = consignLineItem.ConsignSaleLineItemId,
                    Condition = consignLineItem.Condition,
                    Color = consignLineItem.Color,
                    Size = consignLineItem.Size,
                };

                switch (newConsign.Type)
                {

                    case ConsignSaleType.ConsignedForAuction:
                        individualItem = new IndividualAuctionFashionItem()
                        {
                            Note = consignLineItem.Note,
                            CreatedDate = DateTime.UtcNow,
                            MasterItemId = consignDetailRequest.MasterItemId,
                            ItemCode =
                                await _fashionItemRepository.GenerateIndividualItemCode(masterItem.MasterItemCode!),
                            Status = FashionItemStatus.Available,
                            ConsignSaleLineItemId = consignLineItem.ConsignSaleLineItemId,
                            Type = FashionItemType.ConsignedForAuction,
                            Condition = consignLineItem.Condition,
                            Color = consignLineItem.Color,
                            Size = consignLineItem.Size,
                            InitialPrice = consignDetailRequest.ExpectedPrice,
                            SellingPrice = 0,
                        };
                        break;
                    case ConsignSaleType.ConsignedForSale:
                        individualItem.Type = FashionItemType.ConsignedForSale;
                        individualItem.SellingPrice = consignDetailRequest.ExpectedPrice;
                        break;
                    case ConsignSaleType.CustomerSale:
                        individualItem.Type = FashionItemType.CustomerSale;
                        individualItem.SellingPrice = consignDetailRequest.ExpectedPrice;
                        consignLineItem.Status = ConsignSaleLineItemStatus.Sold;
                        newConsign.Status = ConsignSaleStatus.Completed;
                        newConsign.ConsignorReceivedAmount = request.ConsignDetailRequests.Sum(c => c.ExpectedPrice);
                        newConsign.EndDate = DateTime.UtcNow;
                        newConsign.SoldPrice = request.ConsignDetailRequests.Sum(c => c.ExpectedPrice);
                        break;
                }

                individualItem.Images = consignDetailRequest.ImageUrls
                    .Select(url => new Image()
                    {
                        Url = url,
                        CreatedDate = DateTime.UtcNow,
                        IndividualFashionItemId = individualItem.ItemId
                    }).ToList();
                consignLineItem.IndividualFashionItem = individualItem;
                newConsignLineItem.Add(consignLineItem);
            }

            newConsign.ConsignSaleLineItems = newConsignLineItem;
            await _consignSaleRepository.CreateConsignSaleByShop(newConsign);
            if (request.Type == ConsignSaleType.CustomerSale)
            {

                var admin = await _accountRepository.FindOne(c => c.Role == Roles.Admin);
                if (admin is null)
                {
                    throw new AccountNotFoundException();
                }
                admin.Balance += newConsign.SoldPrice;
                var transaction = new Transaction()
                {
                    ConsignSaleId = newConsign.ConsignSaleId,
                    Amount = newConsign.SoldPrice,
                    CreatedDate = DateTime.UtcNow,
                    Type = TransactionType.CustomerSale,
                    PaymentMethod = PaymentMethod.Cash,
                    ShopId = shopId,
                    ReceiverId = admin.AccountId,
                    ReceiverBalance = admin.Balance
                };
                await _transactionRepository.CreateTransaction(transaction);
                await _accountRepository.UpdateAccount(admin);
            }
            else
            {
                await ScheduleConsignEnding(newConsign);
            }
            response.Data = new ConsignSaleDetailedResponse()
            {
                ConsignSaleId = newConsign.ConsignSaleId,
                Status = newConsign.Status,
                SoldPrice = newConsign.SoldPrice,
                TotalPrice = newConsign.TotalPrice,
                CreatedDate = newConsign.CreatedDate,
                ConsignSaleCode = newConsign.ConsignSaleCode,
                ConsignSaleMethod = newConsign.ConsignSaleMethod,
                StartDate = newConsign.StartDate,
                EndDate = newConsign.EndDate,
                Type = newConsign.Type,
                Phone = newConsign.Phone,
                Consginer = newConsign.ConsignorName,
                ShopId = newConsign.ShopId,
                MemberId = newConsign.MemberId,
                MemberReceivedAmount = newConsign.ConsignorReceivedAmount,
                ConsignSaleDetails = newConsign.ConsignSaleLineItems.Select(c => new ConsignSaleDetailResponse2()
                {
                    ConsignSaleLineItemId = c.ConsignSaleLineItemId,
                    Status = c.Status,
                    ConfirmedPrice = c.ConfirmedPrice!.Value,
                    ExpectedPrice = c.ExpectedPrice,
                    DealPrice = c.DealPrice!.Value
                }).ToList()
            };
            response.ResultStatus = ResultStatus.Success;
            response.Messages = ["Create successfully"];
            return response;
        }

        private Expression<Func<ConsignSale, bool>> GetPredicate(ConsignSaleRequest request)
        {
            Expression<Func<ConsignSale, bool>> predicate = x => true;
            if (request.Status != null)
                predicate = predicate.And(sale => sale.Status == request.Status);

            if (request.Type != null)
                predicate = predicate.And(sale => sale.Type == request.Type);

            if (request.StartDate != null)
                predicate = predicate.And(sale => sale.StartDate >= request.StartDate);

            if (request.EndDate != null)
                predicate = predicate.And(sale => sale.EndDate <= request.EndDate);

            if (request.ConsignSaleCode != null)
                predicate = predicate.And(sale =>
                    EF.Functions.ILike(sale.ConsignSaleCode, $"%{request.ConsignSaleCode}%"));


            return predicate;
        }

        public async Task<DotNext.Result<PaginationResponse<ConsignSaleListResponse>, ErrorCode>>
            GetAllConsignSales(Guid accountId,
                ConsignSaleRequest request)
        {
            var query = _consignSaleRepository.GetQueryable();
            Expression<Func<ConsignSale, bool>> predicate = sale => sale.MemberId == accountId;
            predicate = predicate.And(GetPredicate(request));

            var count = await query.Where(predicate).CountAsync();

            var result = await
                query.Where(predicate
                    ).OrderByDescending(x => x.CreatedDate)
                    .Skip(PaginationUtils.GetSkip(request.PageNumber, request.PageSize))
                    .Take(PaginationUtils.GetTake(request.PageSize))
                    .Select(x => new ConsignSaleListResponse()
                    {
                        CreatedDate = x.CreatedDate,
                        ConsignSaleCode = x.ConsignSaleCode,
                        MemberId = x.MemberId,
                        EndDate = x.EndDate,
                        ShopId = x.ShopId,
                        ConsignSaleMethod = x.ConsignSaleMethod,
                        MemberReceivedAmount = x.ConsignorReceivedAmount,
                        Address = x.Address,
                        Type = x.Type,
                        StartDate = x.StartDate,
                        TotalPrice = x.TotalPrice,
                        ConsignSaleId = x.ConsignSaleId,
                        Status = x.Status,
                        SoldPrice = x.SoldPrice,
                        Email = x.Email,
                        Phone = x.Phone,
                        Consginor = x.ConsignorName
                    })
                    .ToListAsync();

            return new DotNext.Result<PaginationResponse<ConsignSaleListResponse>, ErrorCode>(
                new PaginationResponse<ConsignSaleListResponse>()
                {
                    PageNumber = request.PageNumber ?? -1,
                    PageSize = request.PageSize ?? -1,
                    TotalCount = count,
                    Items = result
                });
        }

        public async Task<Result<PaginationResponse<ConsignSaleListResponse>, ErrorCode>> GetConsignSales(
            ConsignSaleListRequest request)
        {
            Expression<Func<ConsignSale, bool>> predicate = consignSale => true;
            Expression<Func<ConsignSale, ConsignSaleListResponse>> selector = consignSale =>
                new ConsignSaleListResponse()
                {
                    MemberId = consignSale.MemberId,
                    CreatedDate = consignSale.CreatedDate,
                    ShopId = consignSale.ShopId,
                    StartDate = consignSale.StartDate,
                    TotalPrice = consignSale.TotalPrice,
                    ConsignSaleId = consignSale.ConsignSaleId,
                    Email = consignSale.Email,
                    Address = consignSale.Address,
                    Type = consignSale.Type,
                    Consginor = consignSale.ConsignorName,
                    MemberReceivedAmount = consignSale.ConsignorReceivedAmount,
                    ConsignSaleMethod = consignSale.ConsignSaleMethod,
                    EndDate = consignSale.EndDate,
                    ConsignSaleCode = consignSale.ConsignSaleCode,
                    Phone = consignSale.Phone,
                    Status = consignSale.Status,
                    SoldPrice = consignSale.SoldPrice
                };

            if (!string.IsNullOrEmpty(request.ConsignSaleCode))
            {
                predicate = predicate.And(x => EF.Functions.ILike(x.ConsignSaleCode, $"%{request.ConsignSaleCode}%"));
            }

            if (request.StartDate != null && request.EndDate != null)
            {
                predicate = predicate.And(x => x.StartDate >= request.StartDate && x.EndDate <= request.EndDate);
            }

            if (request.Status != null)
            {
                predicate = predicate.And(x => x.Status == request.Status);
            }

            if (request.ShopId.HasValue)
            {
                predicate = predicate.And(x => x.ShopId == request.ShopId);
            }

            if (request.Email != null)
            {
                predicate = predicate.And(x => x.Email != null && EF.Functions.ILike(x.Email, $"%{request.Email}%"));
            }

            if (request.ConsignType != null)
            {
                predicate = predicate.And(x => x.Type == request.ConsignType);
            }

            if (request.ConsignorName != null)
            {
                predicate = predicate.And(x =>
                    x.ConsignorName != null && EF.Functions.ILike(x.ConsignorName, $"%{request.ConsignorName}%"));
            }

            if (request.ConsignorPhone != null)
            {
                predicate = predicate.And(x => EF.Functions.Like(x.Phone, $"%{request.ConsignorPhone}%"));
            }

            try
            {
                (List<ConsignSaleListResponse> Items, int Page, int PageSize, int TotalCount) result =
                    await _consignSaleRepository
                        .GetConsignSalesProjections<ConsignSaleListResponse>(predicate, selector, request.Page,
                            request.PageSize);

                return new Result<PaginationResponse<ConsignSaleListResponse>, ErrorCode>(
                    new PaginationResponse<ConsignSaleListResponse>()
                    {
                        Items = result.Items ?? [],
                        TotalCount = result.TotalCount,
                        PageNumber = result.Page,
                        PageSize = result.PageSize,
                        SearchTerm = request.ConsignSaleCode,
                    }
                );
            }
            catch (Exception e)
            {
                return new Result<PaginationResponse<ConsignSaleListResponse>, ErrorCode>(ErrorCode.ServerError);
            }
        }

        public async Task<BusinessObjects.Dtos.Commons.Result<ConsignSaleLineItemsListResponse>>
            ConfirmConsignSaleLineItemPrice(Guid consignLineItemId, decimal price)
        {
            Expression<Func<ConsignSaleLineItem, bool>> predicate = consignLineItem =>
                consignLineItem.ConsignSaleLineItemId == consignLineItemId;
            var consignSaleLine = await _consignSaleLineItemRepository.GetSingleConsignSaleLineItem(predicate);
            if (consignSaleLine is null)
                throw new ConsignSaleLineItemNotFoundException();
            consignSaleLine.ConfirmedPrice = price;
            await _consignSaleLineItemRepository.UpdateConsignLineItem(consignSaleLine);
            return new BusinessObjects.Dtos.Commons.Result<ConsignSaleLineItemsListResponse>()
            {
                Data = new ConsignSaleLineItemsListResponse()
                {
                    ConsignSaleId = consignSaleLine.ConsignSaleId,
                    ConsignSaleLineItemId = consignSaleLine.ConsignSaleLineItemId,
                    ConfirmedPrice = consignSaleLine.ConfirmedPrice
                },
                ResultStatus = ResultStatus.Success,
                Messages = new[] { "Update confirm price for consign line item successfully" }
            };
        }


        public async Task<DotNext.Result<ConsignSaleDetailedResponse, ErrorCode>> GetConsignSaleById(Guid consignId)
        {
            try
            {
                var consignSale = await _consignSaleRepository.GetConsignSaleById(consignId);
                if (consignSale is null)
                {
                    return new DotNext.Result<ConsignSaleDetailedResponse, ErrorCode>(ErrorCode.NotFound);
                }

                return new DotNext.Result<ConsignSaleDetailedResponse, ErrorCode>(consignSale);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error fetching consign sale details");
                return new Result<ConsignSaleDetailedResponse, ErrorCode>(ErrorCode.ServerError);
            }
        }

        public async Task<DotNext.Result<List<ConsignSaleLineItemsListResponse>, ErrorCode>> GetConsignSaleLineItems(
            Guid consignSaleId)
        {
            Expression<Func<ConsignSaleLineItem, bool>> predicate = lineItem => lineItem.ConsignSaleId == consignSaleId;
            Expression<Func<ConsignSaleLineItem, ConsignSaleLineItemsListResponse>> selector = lineItem =>
                new ConsignSaleLineItemsListResponse
                {
                    ConsignSaleLineItemId = lineItem.ConsignSaleLineItemId,
                    ConsignSaleId = lineItem.ConsignSaleId,
                    Status = lineItem.Status,
                    ProductName = lineItem.ProductName,
                    Condition = lineItem.Condition,
                    Brand = lineItem.Brand,
                    Color = lineItem.Color,
                    Gender = lineItem.Gender,
                    Size = lineItem.Size,
                    Images = lineItem.Images.Select(x => x.Url ?? string.Empty).ToList(),
                    ConfirmedPrice = lineItem.ConfirmedPrice,
                    Note = lineItem.Note,
                    ShopResponse = lineItem.ResponseFromShop,
                    CreatedDate = lineItem.CreatedDate,
                    ExpectedPrice = lineItem.ExpectedPrice,
                    DealPrice = lineItem.DealPrice,
                    IsApproved = lineItem.IsApproved,
                    IndividualItemId = lineItem.IndividualFashionItem.ItemId
                };

            try
            {
                var result = await _consignSaleLineItemRepository.GetQueryable()
                    .Where(predicate)
                    .Select(selector)
                    .ToListAsync();

                return new DotNext.Result<List<ConsignSaleLineItemsListResponse>, ErrorCode>(result);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Get consign sale details error");
                return new Result<List<ConsignSaleLineItemsListResponse>, ErrorCode>(ErrorCode.ServerError);
            }
        }

        public async Task<BusinessObjects.Dtos.Commons.Result<MasterItemResponse>>
            CreateMasterItemFromConsignSaleLineItem(Guid consignLineItemId,
                CreateMasterItemForConsignRequest detailRequest)
        {
            var response = new BusinessObjects.Dtos.Commons.Result<MasterItemResponse>();
            var consignSaleLineItem =
                await _consignSaleLineItemRepository.GetSingleConsignSaleLineItem(c =>
                    c.ConsignSaleLineItemId == consignLineItemId);
            if (consignSaleLineItem is null ||
                !consignSaleLineItem.Status.Equals(ConsignSaleLineItemStatus.ReadyForConsignSale))
            {
                throw new ConsignSaleLineItemNotFoundException();
            }

            var masterItem = new MasterFashionItem()
            {
                Brand = consignSaleLineItem.Brand,
                Description = detailRequest.Description,
                Name = detailRequest.Name,
                IsConsignment = true,
                Gender = consignSaleLineItem.Gender,
                ShopId = consignSaleLineItem.ConsignSale.ShopId,
                CategoryId = detailRequest.CategoryId,
                MasterItemCode =
                    await _fashionItemRepository.GenerateConsignMasterItemCode(detailRequest.MasterItemCode,
                        consignSaleLineItem.ConsignSale.ShopId),
                CreatedDate = DateTime.UtcNow
            };

            await _fashionItemRepository.AddSingleMasterFashionItem(masterItem);


            var listImage = new List<Image>();
            foreach (var image in detailRequest.Images)
            {
                var dataImage = new Image()
                {
                    Url = image,
                    CreatedDate = DateTime.UtcNow,
                    MasterFashionItemId = masterItem.MasterItemId
                };
                listImage.Add(dataImage);
            }

            await _imageRepository.AddRangeImage(listImage);
            masterItem.Images = listImage;

            var result = _mapper.Map<MasterItemResponse>(masterItem);
            response.Data = result;
            response.Messages = ["Success"];
            response.ResultStatus = ResultStatus.Success;
            return response;
        }


        public async Task<BusinessObjects.Dtos.Commons.Result<ConsignSaleLineItemResponse>>
            NegotiateConsignSaleLineItem(Guid consignLineItemId, NegotiateConsignSaleLineRequest request)
        {
            Expression<Func<ConsignSaleLineItem, bool>> predicate = consignsaledetail =>
                consignsaledetail.ConsignSaleLineItemId == consignLineItemId;
            var consignSaleDetail = await _consignSaleLineItemRepository.GetSingleConsignSaleLineItem(predicate);
            if (consignSaleDetail == null)
            {
                throw new ConsignSaleLineItemNotFoundException();
            }

            if (request.DealPrice <= 0)
            {
                throw new ConfirmPriceIsNullException("Please set a deal price for this item");
            }

            if (request.DealPrice == consignSaleDetail.ExpectedPrice)
            {
                throw new DealPriceIsNotAvailableException("This deal price is equal expected price");
            }
            if (request.ResponseFromShop is null)
            {
                throw new MissingFeatureException("You should give a reason to negotiate this item");
            }

            consignSaleDetail.Status = ConsignSaleLineItemStatus.Negotiating;
            consignSaleDetail.DealPrice = request.DealPrice;
            consignSaleDetail.ResponseFromShop = request.ResponseFromShop;
            await _consignSaleLineItemRepository.UpdateConsignLineItem(consignSaleDetail);
            return new BusinessObjects.Dtos.Commons.Result<ConsignSaleLineItemResponse>()
            {
                Data = new ConsignSaleLineItemResponse()
                {
                    ConsignSaleLineItemId = consignSaleDetail.ConsignSaleLineItemId,
                    ConsignSaleLineItemStatus = consignSaleDetail.Status,
                    DealPrice = consignSaleDetail.DealPrice!.Value,
                    IsApproved = consignSaleDetail.IsApproved,
                    ResponseFromShop = consignSaleDetail.ResponseFromShop,
                },
                Messages = new[] { "Negotiate individual item price successfully" },
                ResultStatus = ResultStatus.Success
            };
        }

        public async Task<BusinessObjects.Dtos.Commons.Result<ConsignSaleLineItemResponse>> ApproveNegotiation(
            Guid consignLineItemId)
        {
            Expression<Func<ConsignSale, bool>> predicate = consignsale =>
                consignsale.ConsignSaleLineItems.Any(c => c.ConsignSaleLineItemId == consignLineItemId);
            var consignSale = await _consignSaleRepository.GetSingleConsignSale(predicate);
            if (consignSale is null)
            {
                throw new ConsignSaleNotFoundException();
            }

            var consignSaleDetail =
                consignSale.ConsignSaleLineItems.FirstOrDefault(c => c.ConsignSaleLineItemId == consignLineItemId);
            if (consignSaleDetail == null || !consignSaleDetail.Status.Equals(ConsignSaleLineItemStatus.Negotiating))
            {
                throw new ConsignSaleLineItemNotFoundException();
            }

            consignSaleDetail.IsApproved = true;
            consignSaleDetail.Status = ConsignSaleLineItemStatus.ReadyForConsignSale;
            consignSaleDetail.ConfirmedPrice = consignSaleDetail.DealPrice;

            consignSale.TotalPrice = consignSale.ConsignSaleLineItems.Sum(c => c.ConfirmedPrice!.Value);
            await _consignSaleRepository.UpdateConsignSale(consignSale);
            return new BusinessObjects.Dtos.Commons.Result<ConsignSaleLineItemResponse>()
            {
                Data = new ConsignSaleLineItemResponse()
                {
                    ConsignSaleLineItemId = consignSaleDetail.ConsignSaleLineItemId,
                    DealPrice = consignSaleDetail.DealPrice!.Value,
                    ConfirmedPrice = consignSaleDetail.ConfirmedPrice!.Value,
                    IsApproved = consignSaleDetail.IsApproved,
                    ResponseFromShop = consignSaleDetail.ResponseFromShop,
                    ConsignSaleLineItemStatus = consignSaleDetail.Status,
                },
                Messages = new[] { "You have approved deal price of this item from our shop" },
                ResultStatus = ResultStatus.Success
            };
        }

        public async Task<BusinessObjects.Dtos.Commons.Result<ConsignSaleLineItemResponse>> RejectNegotiation(
            Guid consignLineItemId)
        {
            Expression<Func<ConsignSaleLineItem, bool>> predicate = consignsaledetail =>
                consignsaledetail.ConsignSaleLineItemId == consignLineItemId;
            var consignSaleDetail = await _consignSaleLineItemRepository.GetSingleConsignSaleLineItem(predicate);
            var consignSale = await _consignSaleRepository.GetQueryable()
                .FirstOrDefaultAsync(x => x.ConsignSaleId == consignSaleDetail.ConsignSaleId);
            if (consignSaleDetail == null || !consignSaleDetail.Status.Equals(ConsignSaleLineItemStatus.Negotiating))
            {
                throw new ConsignSaleLineItemNotFoundException();
            }


            consignSaleDetail.IsApproved = false;
            consignSaleDetail.Status = ConsignSaleLineItemStatus.Returned;
            await _consignSaleLineItemRepository.UpdateConsignLineItem(consignSaleDetail);

            var consignLineItemCount = await _consignSaleLineItemRepository.GetQueryable()
                .Where(x => x.ConsignSaleId == consignSaleDetail.ConsignSaleId)
                .CountAsync();

            if (consignLineItemCount == 1)
            {
                consignSale.Status = ConsignSaleStatus.Cancelled;
                await _consignSaleRepository.UpdateConsignSale(consignSale);
            }

            return new BusinessObjects.Dtos.Commons.Result<ConsignSaleLineItemResponse>()
            {
                Data = new ConsignSaleLineItemResponse()
                {
                    ConsignSaleLineItemId = consignSaleDetail.ConsignSaleLineItemId,
                    DealPrice = consignSaleDetail.DealPrice!.Value,
                    IsApproved = consignSaleDetail.IsApproved,
                    ResponseFromShop = consignSaleDetail.ResponseFromShop,
                    ConsignSaleLineItemStatus = consignSaleDetail.Status,
                },
                Messages = new[]
                    { "You have rejected deal price of this item from our shop. We will send back your item soon" },
                ResultStatus = ResultStatus.Success
            };
        }

        public async Task<BusinessObjects.Dtos.Commons.Result<ConsignSaleLineItemResponse>>
            CreateIndividualAfterNegotiation(Guid consignLineItemId, CreateIndividualAfterNegotiationRequest request)
        {
            Expression<Func<ConsignSale, bool>> predicate = consignsale =>
                consignsale.ConsignSaleLineItems.Any(c => c.ConsignSaleLineItemId == consignLineItemId);
            var consignSale = await _consignSaleRepository.GetSingleConsignSale(predicate);
            if (consignSale is null)
            {
                throw new ConsignSaleNotFoundException();
            }

            var consignSaleDetail =
                consignSale.ConsignSaleLineItems.FirstOrDefault(c => c.ConsignSaleLineItemId == consignLineItemId);
            if (consignSaleDetail == null)
            {
                throw new ConsignSaleLineItemNotFoundException();
            }

            if (consignSaleDetail.Status != ConsignSaleLineItemStatus.ReadyForConsignSale)
            {
                throw new ItemNotReadyForConsignException();
            }

            Expression<Func<MasterFashionItem, bool>> predicateMaster =
                masterItem => masterItem.MasterItemId == request.MasterItemId;
            var itemMaster = await _fashionItemRepository.GetSingleMasterItem(predicateMaster);
            if (itemMaster is null)
            {
                throw new MasterItemNotAvailableException("Master item is not found");
            }

            if (itemMaster.IsConsignment == false)
            {
                throw new MasterItemNotAvailableException("You can not choose master in stock");
            }


            var individualItem = new IndividualFashionItem()
            {
                Note = consignSaleDetail.Note,
                CreatedDate = DateTime.UtcNow,
                MasterItemId = request.MasterItemId,
                ItemCode = await _fashionItemRepository.GenerateIndividualItemCode(itemMaster.MasterItemCode),
                Status = FashionItemStatus.PendingForConsignSale,
                ConsignSaleLineItemId = consignLineItemId,
                Condition = consignSaleDetail.Condition,

                Color = consignSaleDetail.Color,
                Size = consignSaleDetail.Size
            };

            switch (consignSaleDetail.ConsignSale.Type)
            {
                case ConsignSaleType.CustomerSale:
                    individualItem.Type = FashionItemType.ItemBase;
                    individualItem.SellingPrice = consignSaleDetail.ConfirmedPrice;

                    break;
                case ConsignSaleType.ConsignedForAuction:
                    individualItem = new IndividualAuctionFashionItem()
                    {
                        Note = consignSaleDetail.Note,
                        CreatedDate = DateTime.UtcNow,
                        MasterItemId = request.MasterItemId,
                        ItemCode =
                            await _fashionItemRepository.GenerateIndividualItemCode(itemMaster.MasterItemCode),
                        Status = FashionItemStatus.PendingForConsignSale,
                        ConsignSaleLineItemId = consignLineItemId,
                        Type = FashionItemType.ConsignedForAuction,
                        Condition = consignSaleDetail.Condition,
                        Color = consignSaleDetail.Color,
                        Size = consignSaleDetail.Size,
                        InitialPrice = consignSaleDetail.ConfirmedPrice,
                        SellingPrice = 0,
                    };

                    break;
                case ConsignSaleType.ConsignedForSale:
                    individualItem.Type = FashionItemType.ConsignedForSale;
                    individualItem.SellingPrice = consignSaleDetail.ConfirmedPrice;

                    break;
            }

            individualItem.Images = consignSaleDetail.Images
                .Select(x => new Image()
                {
                    Url = x.Url,
                    CreatedDate = DateTime.UtcNow,
                    IndividualFashionItemId = individualItem.ItemId
                }).ToList();

            consignSaleDetail.IndividualFashionItem = individualItem;

            var listItemInConsign = consignSale.ConsignSaleLineItems
                .Where(c => c.Status == ConsignSaleLineItemStatus.ReadyForConsignSale &&
                            c.IndividualFashionItem != null)
                .ToList();
            if (listItemInConsign.Count ==
                consignSale.ConsignSaleLineItems.Count(c => c.Status == ConsignSaleLineItemStatus.ReadyForConsignSale))
            {
                foreach (var consignSaleLineItem in listItemInConsign)
                {
                    consignSaleLineItem.Status = ConsignSaleLineItemStatus.OnSale;
                    consignSaleLineItem.IndividualFashionItem.Status = FashionItemStatus.Available;
                }

                consignSale.Status = ConsignSaleStatus.OnSale;
                consignSale.StartDate = DateTime.UtcNow;
                consignSale.EndDate = DateTime.UtcNow.AddDays(60);

            }

            await _consignSaleRepository.UpdateConsignSale(consignSale);
            // await _emailService.SendEmailConsignSaleReceived(consignSale);
            return new BusinessObjects.Dtos.Commons.Result<ConsignSaleLineItemResponse>()
            {
                Data = new ConsignSaleLineItemResponse()
                {
                    ConsignSaleLineItemId = consignSaleDetail.ConsignSaleLineItemId,
                    ConsignSaleLineItemStatus = consignSaleDetail.Status,
                    DealPrice = consignSaleDetail.DealPrice!.Value,
                    ConfirmedPrice = consignSaleDetail.ConfirmedPrice!.Value,
                    IsApproved = consignSaleDetail.IsApproved,
                    ResponseFromShop = consignSaleDetail.ResponseFromShop,
                    IndividualItemId = individualItem.ItemId,
                    FashionItemStatus = individualItem.Status
                },
                Messages = new[] { "Create individual item successfully" },
                ResultStatus = ResultStatus.Success
            };
        }

        public async Task<BusinessObjects.Dtos.Commons.Result<ConsignSaleDetailedResponse>> PostConsignSaleForSelling(
            Guid consignSaleId)
        {
            Expression<Func<ConsignSale, bool>> predicate = consignSale => consignSale.ConsignSaleId == consignSaleId;
            var consignSale = await _consignSaleRepository.GetSingleConsignSale(predicate);
            if (consignSale is null || !consignSale.Status.Equals(ConsignSaleStatus.ReadyToSale))
            {
                throw new ConsignSaleNotFoundException();
            }

            var listItemInConsign = consignSale.ConsignSaleLineItems
                .Where(c => c.Status == ConsignSaleLineItemStatus.ReadyForConsignSale)
                .Select(c => c.IndividualFashionItem).ToList();
            if (listItemInConsign.Count <
                consignSale.ConsignSaleLineItems.Count(c => c.Status == ConsignSaleLineItemStatus.ReadyForConsignSale))
            {
                throw new StockCountUnavailableException("Missing item for consign sale line. Please add more");
            }

            foreach (var consignSaleLineItem in consignSale.ConsignSaleLineItems.Where(
                         c => c.IndividualFashionItem != null))
            {
                consignSaleLineItem.Status = ConsignSaleLineItemStatus.OnSale;
                consignSaleLineItem.IndividualFashionItem.Status = FashionItemStatus.Available;
            }

            consignSale.Status = ConsignSaleStatus.OnSale;
            consignSale.StartDate = DateTime.UtcNow;
            consignSale.EndDate = DateTime.UtcNow.AddDays(60);
            await _consignSaleRepository.UpdateConsignSale(consignSale);
            await ScheduleConsignEnding(consignSale);
            return new BusinessObjects.Dtos.Commons.Result<ConsignSaleDetailedResponse>()
            {
                Data = _mapper.Map<ConsignSaleDetailedResponse>(consignSale),
                Messages = new[] { "Post items successfully" },
                ResultStatus = ResultStatus.Success
            };
        }

        public async Task<Result<ConsignSaleDetailedResponse, ErrorCode>> NotifyDelivery(Guid consignsaleId)
        {
            var consignSale = await _consignSaleRepository.GetSingleConsignSale(x => x.ConsignSaleId == consignsaleId);

            if (consignSale == null)
            {
                return new Result<ConsignSaleDetailedResponse, ErrorCode>(ErrorCode.NotFound);
            }

            consignSale.Status = ConsignSaleStatus.AwaitDelivery;

            try
            {
                await _consignSaleRepository.UpdateConsignSale(consignSale);

                var response = new ConsignSaleDetailedResponse()
                {
                    ConsignSaleId = consignSale.ConsignSaleId,
                    Status = consignSale.Status,
                    SoldPrice = consignSale.SoldPrice,
                    CreatedDate = consignSale.CreatedDate,
                    ConsignSaleCode = consignSale.ConsignSaleCode,
                    Phone = consignSale.Phone,
                    Email = consignSale.Email,
                    Address = consignSale.Address,
                    Type = consignSale.Type,
                    StartDate = consignSale.StartDate,
                    TotalPrice = consignSale.TotalPrice,
                    EndDate = consignSale.EndDate,
                    ShopId = consignSale.ShopId,
                    ConsignSaleMethod = consignSale.ConsignSaleMethod,
                    MemberReceivedAmount = consignSale.ConsignorReceivedAmount,
                    MemberId = consignSale.MemberId,
                    Consginer = consignSale.ConsignorName,
                };

                return new Result<ConsignSaleDetailedResponse, ErrorCode>(response);
            }
            catch (Exception e)
            {
                return new Result<ConsignSaleDetailedResponse, ErrorCode>(ErrorCode.ServerError);
            }
        }

        public async Task<Result<ConsignSaleDetailedResponse, ErrorCode>> CancelAllConsignSaleLineItems(
            Guid consignsaleId)
        {
            var consignSale = await _consignSaleRepository.GetSingleConsignSale(x => x.ConsignSaleId == consignsaleId);

            if (consignSale == null)
            {
                return new Result<ConsignSaleDetailedResponse, ErrorCode>(ErrorCode.NotFound);
            }

            if (consignSale.Status != ConsignSaleStatus.Pending && consignSale.Status != ConsignSaleStatus.AwaitDelivery && consignSale.Status != ConsignSaleStatus.Negotiating)
            {
                throw new StatusNotAvailableWithMessageException("Only able to cancel when consign is pending, awaitdelivery or negotiating");
            }
            consignSale.Status = ConsignSaleStatus.Cancelled;

            foreach (var consignSaleLineItem in consignSale.ConsignSaleLineItems)
            {


                consignSaleLineItem.Status = ConsignSaleLineItemStatus.Returned;
            }

            var listConsignSaleLineResponse = consignSale.ConsignSaleLineItems.Select(c =>
                new ConsignSaleDetailResponse2()
                {
                    ConsignSaleLineItemId = c.ConsignSaleLineItemId,
                    ConfirmedPrice = c.ConfirmedPrice!.Value,
                    DealPrice = c.DealPrice!.Value,
                    ExpectedPrice = c.ExpectedPrice,
                    ConsignSaleId = c.ConsignSaleId,
                    Status = c.Status,
                    Note = c.Note
                }).ToList();
            try
            {
                await _consignSaleRepository.UpdateConsignSale(consignSale);

                var response = new ConsignSaleDetailedResponse()
                {
                    ConsignSaleId = consignSale.ConsignSaleId,
                    Status = consignSale.Status,
                    SoldPrice = consignSale.SoldPrice,
                    CreatedDate = consignSale.CreatedDate,
                    ConsignSaleCode = consignSale.ConsignSaleCode,
                    Phone = consignSale.Phone,
                    Email = consignSale.Email,
                    Address = consignSale.Address,
                    Type = consignSale.Type,
                    StartDate = consignSale.StartDate,
                    TotalPrice = consignSale.TotalPrice,
                    EndDate = consignSale.EndDate,
                    ShopId = consignSale.ShopId,
                    ConsignSaleMethod = consignSale.ConsignSaleMethod,
                    MemberReceivedAmount = consignSale.ConsignorReceivedAmount,
                    MemberId = consignSale.MemberId,
                    Consginer = consignSale.ConsignorName,
                    ConsignSaleDetails = listConsignSaleLineResponse
                };

                return new Result<ConsignSaleDetailedResponse, ErrorCode>(response);
            }
            catch (Exception e)
            {
                return new Result<ConsignSaleDetailedResponse, ErrorCode>(ErrorCode.ServerError);
            }
        }

        public async Task<BusinessObjects.Dtos.Commons.Result<ConsignSaleDetailedResponse>> NegotiatingConsignSale(
            Guid consignSaleId)
        {
            Expression<Func<ConsignSale, bool>> predicate = consignSale => consignSale.ConsignSaleId == consignSaleId;
            var consignSale = await _consignSaleRepository.GetSingleConsignSale(predicate);
            if (consignSale is null ||
                consignSale.ConsignSaleLineItems.All(c => c.Status != ConsignSaleLineItemStatus.Negotiating))
            {
                throw new ConsignSaleNotFoundException();
            }

            consignSale.Status = ConsignSaleStatus.Negotiating;
            await _consignSaleRepository.UpdateConsignSale(consignSale);
            await _emailService.SendEmailConsignNegotiatePrice(consignSale);
            var response = new ConsignSaleDetailedResponse()
            {
                ConsignSaleId = consignSale.ConsignSaleId,
                Status = consignSale.Status,
                SoldPrice = consignSale.SoldPrice,
                CreatedDate = consignSale.CreatedDate,
                ConsignSaleCode = consignSale.ConsignSaleCode,
                Phone = consignSale.Phone,
                Email = consignSale.Email,
                Address = consignSale.Address,
                Type = consignSale.Type,
                StartDate = consignSale.StartDate,
                TotalPrice = consignSale.TotalPrice,
                EndDate = consignSale.EndDate,
                ShopId = consignSale.ShopId,
                ConsignSaleMethod = consignSale.ConsignSaleMethod,
                MemberReceivedAmount = consignSale.ConsignorReceivedAmount,
                MemberId = consignSale.MemberId,
                Consginer = consignSale.ConsignorName,
            };
            return new BusinessObjects.Dtos.Commons.Result<ConsignSaleDetailedResponse>()
            {
                Data = response,
                Messages = new[] { "Send negotiating successfully" },
                ResultStatus = ResultStatus.Success
            };
        }

        public async Task<BusinessObjects.Dtos.Commons.Result<ConsignSaleDetailedResponse>> ReadyToSaleConsignSale(
            Guid consignSaleId)
        {
            Expression<Func<ConsignSale, bool>> predicate = consignSale => consignSale.ConsignSaleId == consignSaleId;
            var consignSale = await _consignSaleRepository.GetSingleConsignSale(predicate);
            if (consignSale is null || consignSale.Status == ConsignSaleStatus.Negotiating)
            {
                throw new ConsignSaleNotFoundException();
            }

            if (consignSale.ConsignSaleLineItems.Any(c => c.Status != ConsignSaleLineItemStatus.ReadyForConsignSale))
            {
                throw new ConsignSaleLineItemNotAvailableException("You must update all consign sale line item");
            }

            consignSale.Status = ConsignSaleStatus.ReadyToSale;
            consignSale.TotalPrice = consignSale.ConsignSaleLineItems
                .Where(c => c.Status == ConsignSaleLineItemStatus.ReadyForConsignSale)
                .Sum(c => c.ConfirmedPrice!.Value);
            await _consignSaleRepository.UpdateConsignSale(consignSale);
            // await _emailService.SnedMailNegotiating(consignSale);
            var response = new ConsignSaleDetailedResponse()
            {
                ConsignSaleId = consignSale.ConsignSaleId,
                Status = consignSale.Status,
                SoldPrice = consignSale.SoldPrice,
                CreatedDate = consignSale.CreatedDate,
                ConsignSaleCode = consignSale.ConsignSaleCode,
                Phone = consignSale.Phone,
                Email = consignSale.Email,
                Address = consignSale.Address,
                Type = consignSale.Type,
                StartDate = consignSale.StartDate,
                TotalPrice = consignSale.TotalPrice,
                EndDate = consignSale.EndDate,
                ShopId = consignSale.ShopId,
                ConsignSaleMethod = consignSale.ConsignSaleMethod,
                MemberReceivedAmount = consignSale.ConsignorReceivedAmount,
                MemberId = consignSale.MemberId,
                Consginer = consignSale.ConsignorName,
            };
            return new BusinessObjects.Dtos.Commons.Result<ConsignSaleDetailedResponse>()
            {
                Data = response,
                Messages = new[] { "Confirm consign sale line item successfully" },
                ResultStatus = ResultStatus.Success
            };
        }

        public async Task<Result<ConsignSaleDetailedResponse, ErrorCode>> ContinueConsignSale(Guid consignsaleId)
        {
            var consignSale = await _consignSaleRepository.GetSingleConsignSale(x => x.ConsignSaleId == consignsaleId);

            if (consignSale == null)
            {
                return new Result<ConsignSaleDetailedResponse, ErrorCode>(ErrorCode.NotFound);
            }

            consignSale.Status = ConsignSaleStatus.ReadyToSale;

            var listConsignSaleLineResponse = consignSale.ConsignSaleLineItems.Select(c =>
                new ConsignSaleDetailResponse2()
                {
                    ConsignSaleLineItemId = c.ConsignSaleLineItemId,
                    ConfirmedPrice = c.ConfirmedPrice!.Value,
                    DealPrice = c.DealPrice!.Value,
                    ExpectedPrice = c.ExpectedPrice,
                    ConsignSaleId = c.ConsignSaleId,
                    Status = c.Status,
                    Note = c.Note
                }).ToList();
            if (listConsignSaleLineResponse.Any(c =>
                    c.Status != ConsignSaleLineItemStatus.ReadyForConsignSale &&
                    c.Status != ConsignSaleLineItemStatus.Returned))
            {
                throw new ConsignSaleLineItemNotAvailableException(
                    "You need to confirm ready for all consign sale line item");
            }

            consignSale.TotalPrice = consignSale.ConsignSaleLineItems
                .Where(c => c.Status == ConsignSaleLineItemStatus.ReadyForConsignSale)
                .Sum(c => c.ConfirmedPrice!.Value);
            try
            {
                await _consignSaleRepository.UpdateConsignSale(consignSale);

                var response = new ConsignSaleDetailedResponse()
                {
                    ConsignSaleId = consignSale.ConsignSaleId,
                    Status = consignSale.Status,
                    SoldPrice = consignSale.SoldPrice,
                    CreatedDate = consignSale.CreatedDate,
                    ConsignSaleCode = consignSale.ConsignSaleCode,
                    Phone = consignSale.Phone,
                    Email = consignSale.Email,
                    Address = consignSale.Address,
                    Type = consignSale.Type,
                    StartDate = consignSale.StartDate,
                    TotalPrice = consignSale.TotalPrice,
                    EndDate = consignSale.EndDate,
                    ShopId = consignSale.ShopId,
                    ConsignSaleMethod = consignSale.ConsignSaleMethod,
                    MemberReceivedAmount = consignSale.ConsignorReceivedAmount,
                    MemberId = consignSale.MemberId,
                    Consginer = consignSale.ConsignorName,
                    ConsignSaleDetails = listConsignSaleLineResponse
                };

                return new Result<ConsignSaleDetailedResponse, ErrorCode>(response);
            }
            catch (Exception e)
            {
                return new Result<ConsignSaleDetailedResponse, ErrorCode>(ErrorCode.ServerError);
            }
        }



        public async Task<Result<InvoiceConsignResponse, ErrorCode>> GenerateConsignOfflineInvoice(Guid consignsaleId, Guid shopId)
        {
            try
            {
                var consignSale = await _consignSaleRepository.GetQueryable()
                    .FirstOrDefaultAsync(c => c.ConsignSaleId == consignsaleId);
                if (consignSale == null)
                {
                    return new Result<InvoiceConsignResponse, ErrorCode>(ErrorCode.NotFound);
                }

                var shop = await _shopRepository.GetShopById(shopId);

                var consignLineItem = await _consignSaleLineItemRepository.GetQueryable()
                    .Include(x => x.IndividualFashionItem)
                    .ThenInclude(x => x.MasterItem)
                    .Where(x => x.ConsignSale.ConsignSaleId == consignsaleId)
                    .Select(x => new ConsignSaleLineItemDetailedResponse()
                    {
                        ProductName = x.IndividualFashionItem.MasterItem.Name,
                        ConfirmedPrice = x.ConfirmedPrice,
                        ItemCode = x.IndividualFashionItem.ItemCode,
                    })
                    .ToListAsync();

                var invoiceHtml = await GenerateInvoiceHtml(consignSale, consignLineItem, shop);
                var renderer = new ChromePdfRenderer()
                {
                    RenderingOptions = new ChromePdfRenderOptions()
                    {
                        CssMediaType = PdfCssMediaType.Print,
                        CustomCssUrl = "https://cdn.jsdelivr.net/npm/bootstrap@5.0.2/dist/css/bootstrap.min.css"
                    }
                };
                var pdf = await renderer.RenderHtmlAsPdfAsync(invoiceHtml);

                var fileName = $"Invoice_{consignSale.ConsignSaleCode}.pdf";
                var filePath = Path.Combine(Path.GetTempPath(), fileName);
                pdf.SaveAs(filePath);

                return new Result<InvoiceConsignResponse, ErrorCode>(new InvoiceConsignResponse
                {
                    Content = pdf.BinaryData,
                    ConsignSaleCode = consignSale.ConsignSaleCode
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating invoice for consign {ConsignSaleId}", consignsaleId);
                return new Result<InvoiceConsignResponse, ErrorCode>(ErrorCode.ServerError);
            }
        }
        private async Task<string> GenerateInvoiceHtml(ConsignSale consignSale, List<ConsignSaleLineItemDetailedResponse> consignLineItems,
            ShopDetailResponse shop)
        {
            var templatePath = Path.Combine("InvoiceTemplate", "consign-invoice.html");
            var template = await File.ReadAllTextAsync(templatePath);

            template = template.Replace("{{InvoiceNumber}}", consignSale.ConsignSaleCode)
                .Replace("{{IssueDate}}", consignSale.CreatedDate.AddHours(7).ToString("dd/MM/yyyy HH:mm:ss"))
                .Replace("{{PaymentMethod}}", PaymentMethod.Cash.ToString())
                .Replace("{{CustomerName}}", consignSale.ConsignorName ?? (consignSale.Member != null ? consignSale.Member.Fullname : "N?A"))
                .Replace("{{CustomerAddress}}", consignSale.Address ?? "N/A")
                .Replace("{{CustomerPhone}}", consignSale.Phone ?? "N/A")
                .Replace("{{CustomerEmail}}", consignSale.Email ?? "N/A")
                .Replace("{{ShopAddress}}", shop.Address ?? "N/A")
                .Replace("{{ShopPhone}}", shop.Phone ?? "N/A");

            var itemsHtml = new StringBuilder();
            foreach (var item in consignLineItems)
            {
                itemsHtml.Append($@"
        <tr>
            <td>{item.ItemCode ?? "N/A"}</td>
            <td>{item.ProductName ?? "N/A"}</td>
            <td class='text-end'>1</td>
            <td class='text-end'>{item.ConfirmedPrice:N0} VND</td>
            <td class='text-end'>{item.ConfirmedPrice:N0} VND</td>
        </tr>");
            }

            template = template.Replace("{{ConsignLineItems}}", itemsHtml.ToString());

            // Replace totals
            template = template.Replace("{{Subtotal}}", $"{consignLineItems.Sum(x => x.ConfirmedPrice):N0}")
                .Replace("{{Total}}", $"{consignSale.TotalPrice:N0}");

            return template;
        }

        public async Task<BusinessObjects.Dtos.Commons.Result<ConsignSaleLineItemResponse>>
            ConfirmConsignSaleLineReadyToSale(Guid consignLineItemId,
                ConfirmConsignSaleLineReadyToSaleRequest request)
        {
            Expression<Func<ConsignSale, bool>> predicate = consignsale =>
                consignsale.ConsignSaleLineItems.Any(c => c.ConsignSaleLineItemId == consignLineItemId);
            var consignSale = await _consignSaleRepository.GetSingleConsignSale(predicate);
            if (consignSale is null)
            {
                throw new ConsignSaleNotFoundException();
            }

            var consignSaleDetail =
                consignSale.ConsignSaleLineItems.FirstOrDefault(c => c.ConsignSaleLineItemId == consignLineItemId);
            if (consignSaleDetail == null)
            {
                throw new ConsignSaleLineItemNotFoundException();
            }

            if (request.DealPrice <= 0)
            {
                throw new ConfirmPriceIsNullException("Please set a deal price for this item");
            }

            if (!consignSaleDetail.ExpectedPrice.Equals(request.DealPrice))
            {
                throw new DealPriceIsNotAvailableException("This deal price is not equal expected price");
            }

            consignSaleDetail.Status = ConsignSaleLineItemStatus.ReadyForConsignSale;
            consignSaleDetail.DealPrice = request.DealPrice;
            consignSaleDetail.ConfirmedPrice = request.DealPrice;
            consignSaleDetail.IsApproved = true;
            if (consignSale.ConsignSaleLineItems.All(
                    c => c.Status.Equals(ConsignSaleLineItemStatus.ReadyForConsignSale)))
            {
                consignSale.Status = ConsignSaleStatus.ReadyToSale;
            }

            await _consignSaleLineItemRepository.UpdateConsignLineItem(consignSaleDetail);

            return new BusinessObjects.Dtos.Commons.Result<ConsignSaleLineItemResponse>()
            {
                Data = new ConsignSaleLineItemResponse()
                {
                    ConsignSaleLineItemId = consignSaleDetail.ConsignSaleLineItemId,
                    ConsignSaleLineItemStatus = consignSaleDetail.Status,
                    DealPrice = consignSaleDetail.DealPrice!.Value,
                    IsApproved = consignSaleDetail.IsApproved,
                    ResponseFromShop = consignSaleDetail.ResponseFromShop,
                    ConfirmedPrice = consignSaleDetail.ConfirmedPrice!.Value
                    /*IndividualItemId = individualItem.ItemId,
                    FashionItemStatus = individualItem.Status*/
                },
                Messages = new[] { "Create individual item successfully" },
                ResultStatus = ResultStatus.Success
            };
        }

        public async Task UpdateConsignPrice(Guid orderId)
        {
            var order = await _orderRepository.GetSingleOrder(c => c.OrderId == orderId);
            foreach (var detail in order.OrderLineItems)
            {
                // var consign =
                //     await _consignSaleRepository.GetSingleConsignSale(c => c.ConsignSaleDetails.Any(c => c.FashionItemId.Equals(detail.IndividualFashionItemId)));
                // if (consign != null)
                // {
                //     consign.SoldPrice += detail.UnitPrice;
                //     if (consign.SoldPrice < 1000000)
                //     {
                //         consign.ConsignorReceivedAmount = consign.SoldPrice * 74 / 100;
                //     }else if (consign.SoldPrice >= 1000000 && consign.SoldPrice <= 10000000)
                //     {
                //         consign.ConsignorReceivedAmount = consign.SoldPrice * 77 / 100;
                //     }
                //     else
                //     {
                //         consign.ConsignorReceivedAmount = consign.SoldPrice * 80 / 100;
                //     }
                //     await _consignSaleRepository.UpdateConsignSale(consign);
            }
        }

        public async Task<Result<ExcelResponse, ErrorCode>> ExportConsignSaleToExcel(ExportConsignSaleToExcelRequest request)
        {
            Expression<Func<ConsignSale, bool>> predicate = consignSale => true;

            if (request.ConsignSaleCode != null)
            {
                predicate = predicate.And(x => EF.Functions.ILike(x.ConsignSaleCode, $"%{request.ConsignSaleCode}%"));
            }

            if (request.MemberName != null)
            {
                predicate = predicate.And(x => x.ConsignorName != null && EF.Functions.ILike(x.ConsignorName, $"%{request.MemberName}%"));
            }

            if (request.Phone != null)
            {
                predicate = predicate.And(x => x.Phone != null && EF.Functions.ILike(x.Phone, $"%{request.Phone}%"));
            }

            if (request.ShopId != null)
            {
                predicate = predicate.And(x => x.ShopId == request.ShopId);
            }


            if (request.Email != null)
            {
                predicate = predicate.And(x => x.Email != null && EF.Functions.ILike(x.Email, $"%{request.Email}%"));
            }

            if (request.Statuses != null && request.Statuses.Any())
            {
                predicate = predicate.And(x => request.Statuses.Contains(x.Status));
            }

            if (request.Types != null && request.Types.Any())
            {
                predicate = predicate.And(x => request.Types.Contains(x.Type));
            }

            if (request.ConsignSaleMethods != null && request.ConsignSaleMethods.Any())
            {
                predicate = predicate.And(x => request.ConsignSaleMethods.Contains(x.ConsignSaleMethod));
            }

            if (request.StartDate != null)
            {
                predicate = predicate.And(x => x.CreatedDate >= request.StartDate);
            }

            if (request.EndDate != null)
            {
                predicate = predicate.And(x => x.CreatedDate <= request.EndDate);
            }

            var consignSales = await _consignSaleRepository.GetQueryable()
            .Where(predicate)
            .Select(x => new
            {
                ConsignSaleCode = x.ConsignSaleCode,
                ConsignorName = x.ConsignorName,
                Member = x.Member != null ? x.Member.Fullname : null,
                Phone = x.Phone,
                Email = x.Email,
                Address = x.Address,
                ShopId = x.ShopId,
                ShopAddress = x.Shop.Address,
                Status = x.Status,
                Type = x.Type,
                ConsignSaleMethod = x.ConsignSaleMethod,
                CreatedDate = x.CreatedDate,
                StartDate = x.StartDate,
                EndDate = x.EndDate,
                ConsignorReceivedAmount = x.ConsignorReceivedAmount,
                TotalPrice = x.TotalPrice,
                SoldPrice = x.SoldPrice,
                ResponseFromShop = x.ResponseFromShop,
            })
            .ToListAsync();

            if (!consignSales.Any())
            {
                return new Result<ExcelResponse, ErrorCode>(ErrorCode.NotFound);
            }

            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Consign Sales");


                using (var range = worksheet.Cells["A1:K1"])
                {
                    range.Style.Font.Bold = true;
                    range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightBlue);
                    range.Style.Font.Color.SetColor(System.Drawing.Color.White);
                }

                worksheet.Cells[1, 1].Value = "Consign Sale Code";
                worksheet.Cells[1, 2].Value = "Consignor Name";
                worksheet.Cells[1, 3].Value = "Member";
                worksheet.Cells[1, 4].Value = "Phone";
                worksheet.Cells[1, 5].Value = "Email";
                worksheet.Cells[1, 6].Value = "Address";
                worksheet.Cells[1, 7].Value = "Shop Address";
                worksheet.Cells[1, 8].Value = "Status";
                worksheet.Cells[1, 9].Value = "Type";
                worksheet.Cells[1, 10].Value = "Consign Sale Method";
                worksheet.Cells[1, 11].Value = "Created Date";
                worksheet.Cells[1, 12].Value = "Start Date";
                worksheet.Cells[1, 13].Value = "End Date";
                worksheet.Cells[1, 14].Value = "Consignor Received Amount";
                worksheet.Cells[1, 15].Value = "Total Price";
                worksheet.Cells[1, 16].Value = "Sold Price";
                worksheet.Cells[1, 17].Value = "Response From Shop";

                int row = 2;
                foreach (var consignSale in consignSales)
                {
                    worksheet.Cells[row, 1].Value = consignSale.ConsignSaleCode;
                    worksheet.Cells[row, 2].Value = consignSale.ConsignorName;
                    worksheet.Cells[row, 3].Value = consignSale.Member;
                    worksheet.Cells[row, 4].Value = consignSale.Phone;
                    worksheet.Cells[row, 5].Value = consignSale.Email;
                    worksheet.Cells[row, 6].Value = consignSale.Address;
                    worksheet.Cells[row, 7].Value = consignSale.ShopAddress;
                    worksheet.Cells[row, 8].Value = consignSale.Status.ToString();
                    worksheet.Cells[row, 9].Value = consignSale.Type.ToString();
                    worksheet.Cells[row, 10].Value = consignSale.ConsignSaleMethod.ToString();
                    worksheet.Cells[row, 11].Value = consignSale.CreatedDate.ToString("dd/MM/yyyy HH:mm:ss");
                    worksheet.Cells[row, 12].Value = consignSale.StartDate?.ToString("dd/MM/yyyy HH:mm:ss");
                    worksheet.Cells[row, 13].Value = consignSale.EndDate?.ToString("dd/MM/yyyy HH:mm:ss");
                    worksheet.Cells[row, 14].Value = consignSale.ConsignorReceivedAmount;
                    worksheet.Cells[row, 15].Value = consignSale.TotalPrice;
                    worksheet.Cells[row, 16].Value = consignSale.SoldPrice;
                    worksheet.Cells[row, 17].Value = consignSale.ResponseFromShop;

                    // Alternate row colors
                    if (row % 2 == 0)
                    {
                        worksheet.Cells[row, 1, row, 17].Style.Fill.PatternType = ExcelFillStyle.Solid;
                        worksheet.Cells[row, 1, row, 17].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                    }

                    row++;
                }

                //Auto fit columns
                worksheet.Cells[1, 1, row - 1, 17].AutoFitColumns();

                //Add borders
                var borderStyle = worksheet.Cells[1, 1, row - 1, 17].Style.Border;
                borderStyle.Top.Style = borderStyle.Left.Style =
                    borderStyle.Right.Style = borderStyle.Bottom.Style = ExcelBorderStyle.Thin;

                //Add title
                worksheet.InsertRow(1, 2);
                worksheet.Cells["A1:K1"].Merge = true;
                worksheet.Cells["A1"].Value = request.ShopId.HasValue ? $"Consign Sale Report for {consignSales.FirstOrDefault()?.ShopAddress}" : "Consign Sale Report";
                worksheet.Cells["A1"].Style.Font.Size = 16;
                worksheet.Cells["A1"].Style.Font.Bold = true;
                worksheet.Cells["A1"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                var excelBytes = package.GetAsByteArray();
                return new Result<ExcelResponse, ErrorCode>(new ExcelResponse()
                {
                    Content = excelBytes,
                    FileName = $"ConsignSales_{DateTime.Now:yyyyMMddHHmmss}.xlsx"
                });
            }
        }

    }
}