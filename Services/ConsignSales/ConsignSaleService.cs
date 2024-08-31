using System.Linq.Expressions;
using AutoMapper;
using BusinessObjects.Dtos.Commons;
using BusinessObjects.Dtos.ConsignSaleLineItems;
using BusinessObjects.Dtos.ConsignSales;
using BusinessObjects.Dtos.Email;
using BusinessObjects.Dtos.FashionItems;
using BusinessObjects.Entities;
using BusinessObjects.Utils;
using DotNext;
using LinqKit;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Org.BouncyCastle.Asn1.Ocsp;
using Quartz;
using Repositories.Accounts;
using Repositories.ConsignSaleLineItems;
using Repositories.ConsignSales;
using Repositories.FashionItems;
using Repositories.Images;
using Repositories.Orders;
using Repositories.Schedules;
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

        public ConsignSaleService(IConsignSaleRepository consignSaleRepository, IAccountRepository accountRepository,
            IConsignSaleLineItemRepository consignSaleLineItemRepository
            , IOrderRepository orderRepository, IEmailService emailService, IMapper mapper,
            ISchedulerFactory schedulerFactory, IFashionItemRepository fashionItemRepository,
            IImageRepository imageRepository, ILogger<ConsignSaleService> logger)
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
            await _emailService.SendEmailConsignSale(consignId);
            response.Messages = ["Approval successfully"];
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
            await _emailService.SendEmailConsignSaleReceived(consignId);
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
            var isMemberExisted = await _accountRepository.FindUserByPhone(request.Phone);
            if (isMemberExisted != null)
            {
                var account = await _accountRepository.GetAccountById(isMemberExisted.AccountId);
                if (account == null || account.Status.Equals(AccountStatus.Inactive) ||
                    account.Status.Equals(AccountStatus.NotVerified))
                {
                    response.Messages = ["This account is not available to consign"];
                    response.ResultStatus = ResultStatus.Error;
                    return response;
                }
            }

            //tao moi' 1 consign form
            var consign = await _consignSaleRepository.CreateConsignSaleByShop(shopId, request);
            if (consign == null)
            {
                response.Messages = ["There is an error. Can not find consign"];
                response.ResultStatus = ResultStatus.Error;
                return response;
            }

            // await ScheduleConsignEnding(consign);
            response.Data = consign;
            response.ResultStatus = ResultStatus.Success;
            response.Messages = ["Create successfully"];
            return response;
        }

        public async Task<BusinessObjects.Dtos.Commons.Result<PaginationResponse<ConsignSaleDetailedResponse>>>
            GetAllConsignSales(Guid accountId,
                ConsignSaleRequest request)
        {
            var response = new BusinessObjects.Dtos.Commons.Result<PaginationResponse<ConsignSaleDetailedResponse>>();
            var listConsign = await _consignSaleRepository.GetAllConsignSale(accountId, request);
            if (listConsign == null)
            {
                response.Messages = ["You don't have any consignment"];
                response.ResultStatus = ResultStatus.Success;
                return response;
            }

            response.Data = listConsign;
            response.Messages = ["There are " + listConsign.TotalCount + " consignment"];
            response.ResultStatus = ResultStatus.Success;
            return response;
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
                _logger.LogError(e,"Error fetching consign sale details");
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
                    ExpectedPrice = lineItem.ExpectedPrice
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
            if (consignSaleLineItem is null || !consignSaleLineItem.Status.Equals(ConsignSaleLineItemStatus.Received))
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
                        consignSaleLineItem.ConsignSale.Shop.ShopCode),
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
                    MasterFashionItemId = masterItem.MasterItemId,
                    ConsignLineItemId = consignLineItemId
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

        public async Task<BusinessObjects.Dtos.Commons.Result<ConsignSaleLineItemResponse>> ApproveNegotiation(Guid consignLineItemId)
        {
            Expression<Func<ConsignSaleLineItem, bool>> predicate = consignsaledetail =>
                consignsaledetail.ConsignSaleLineItemId == consignLineItemId;
            var consignSaleDetail = await _consignSaleLineItemRepository.GetSingleConsignSaleLineItem(predicate);
            if (consignSaleDetail == null || !consignSaleDetail.Status.Equals(ConsignSaleLineItemStatus.Negotiating))
            {
                throw new ConsignSaleLineItemNotFoundException();
            }

            consignSaleDetail.IsApproved = true;
            consignSaleDetail.Status = ConsignSaleLineItemStatus.ReadyForConsignSale;
            consignSaleDetail.ConfirmedPrice = consignSaleDetail.DealPrice;
            await _consignSaleLineItemRepository.UpdateConsignLineItem(consignSaleDetail);
            return new BusinessObjects.Dtos.Commons.Result<ConsignSaleLineItemResponse>()
            {
                Data = new ConsignSaleLineItemResponse()
                {
                    ConsignSaleLineItemId  = consignSaleDetail.ConsignSaleLineItemId,
                    DealPrice = consignSaleDetail.DealPrice!.Value,
                    IsApproved = consignSaleDetail.IsApproved,
                    ResponseFromShop = consignSaleDetail.ResponseFromShop,
                    ConsignSaleLineItemStatus = consignSaleDetail.Status,
                },
                Messages = new []{"You have approved deal price of this item from our shop"},
                ResultStatus = ResultStatus.Success
            };
        }

        public async Task<BusinessObjects.Dtos.Commons.Result<ConsignSaleLineItemResponse>> RejectNegotiation(Guid consignLineItemId)
        {
            Expression<Func<ConsignSaleLineItem, bool>> predicate = consignsaledetail =>
                consignsaledetail.ConsignSaleLineItemId == consignLineItemId;
            var consignSaleDetail = await _consignSaleLineItemRepository.GetSingleConsignSaleLineItem(predicate);
            if (consignSaleDetail == null || !consignSaleDetail.Status.Equals(ConsignSaleLineItemStatus.Negotiating))
            {
                throw new ConsignSaleLineItemNotFoundException();
            }

            consignSaleDetail.IsApproved = false;
            consignSaleDetail.Status = ConsignSaleLineItemStatus.Returned;
            await _consignSaleLineItemRepository.UpdateConsignLineItem(consignSaleDetail);
            return new BusinessObjects.Dtos.Commons.Result<ConsignSaleLineItemResponse>()
            {
                Data = new ConsignSaleLineItemResponse()
                {
                    ConsignSaleLineItemId  = consignSaleDetail.ConsignSaleLineItemId,
                    DealPrice = consignSaleDetail.DealPrice!.Value,
                    IsApproved = consignSaleDetail.IsApproved,
                    ResponseFromShop = consignSaleDetail.ResponseFromShop,
                    ConsignSaleLineItemStatus = consignSaleDetail.Status,
                },
                Messages = new []{"You have rejected deal price of this item from our shop. We will send back your item soon"},
                ResultStatus = ResultStatus.Success
            };
        }

        public async Task<BusinessObjects.Dtos.Commons.Result<ConsignSaleLineItemResponse>> CreateIndividualAfterNegotiation(Guid consignLineItemId, CreateIndividualAfterNegotiationRequest request)
        {
            Expression<Func<ConsignSaleLineItem, bool>> predicate = consignsaledetail =>
                consignsaledetail.ConsignSaleLineItemId == consignLineItemId;
            var consignSaleDetail = await _consignSaleLineItemRepository.GetSingleConsignSaleLineItem(predicate);
            if (consignSaleDetail == null || !consignSaleDetail.Status.Equals(ConsignSaleLineItemStatus.Negotiating))
            {
                throw new ConsignSaleLineItemNotFoundException();
            }
            Expression<Func<MasterFashionItem, bool>> predicateMaster =
                masterItem => masterItem.MasterItemId == request.MasterItemId;
            var itemMaster = await _fashionItemRepository.GetSingleMasterItem(predicateMaster);
            if (itemMaster is null)
            {
                throw new MasterItemNotAvailableException("Master item is not found");
            }

            if (!itemMaster.Images.Select(c => c.ConsignLineItemId).Distinct().Contains(consignLineItemId))
            {
                throw new MasterItemNotAvailableException("This master item is not allowed to use");
            }

            itemMaster.StockCount += 1;
            await _fashionItemRepository.UpdateMasterItem(itemMaster);
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
                case ConsignSaleType.ForSale:
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
                        InitialPrice = consignSaleDetail.ConfirmedPrice,
                        SellingPrice = 0,
                    };
                    break;
                case ConsignSaleType.ConsignedForSale:
                    individualItem.Type = FashionItemType.ConsignedForSale;
                    individualItem.SellingPrice = consignSaleDetail.ConfirmedPrice;
                    break;
            }

            await _consignSaleLineItemRepository.UpdateConsignLineItem(consignSaleDetail);
            await _fashionItemRepository.AddInvidualFashionItem(individualItem);
            foreach (var imageRequest in consignSaleDetail.Images)
            {
                imageRequest.IndividualFashionItemId = individualItem.ItemId;
                await _imageRepository.UpdateSingleImage(imageRequest);
            }
            return new BusinessObjects.Dtos.Commons.Result<ConsignSaleLineItemResponse>()
            {
                Data = new ConsignSaleLineItemResponse()
                {
                    ConsignSaleLineItemId = consignSaleDetail.ConsignSaleLineItemId,
                    ConsignSaleLineItemStatus = consignSaleDetail.Status,
                    DealPrice = consignSaleDetail.DealPrice!.Value,
                    IsApproved = consignSaleDetail.IsApproved,
                    ResponseFromShop = consignSaleDetail.ResponseFromShop,
                    IndividualItemId = individualItem.ItemId,
                    FashionItemStatus = individualItem.Status
                },
                Messages = new[] { "Create individual item successfully" },
                ResultStatus = ResultStatus.Success
            };
        }

        public async Task<BusinessObjects.Dtos.Commons.Result<ConsignSaleDetailedResponse>> PostConsignSaleForSelling(Guid consignSaleId)
        {
            Expression<Func<ConsignSale, bool>> predicate = consignSale => consignSale.ConsignSaleId == consignSaleId;
            var consignSale = await _consignSaleRepository.GetSingleConsignSale(predicate);
            if (consignSale is null || !consignSale.Status.Equals(ConsignSaleStatus.Processing))
            {
                throw new ConsignSaleNotFoundException();
            }

            foreach (var consignSaleLineItem in consignSale.ConsignSaleLineItems)
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
                Messages = new []{"Post items successfully"},
                ResultStatus = ResultStatus.Success
            };  
        }

        public async Task<BusinessObjects.Dtos.Commons.Result<ConsignSaleLineItemResponse>>
            CreateIndividualItemFromConsignSaleLineItem(Guid consignsaledetailId,
                CreateIndividualItemRequestForConsign request)
        {
            Expression<Func<ConsignSaleLineItem, bool>> predicate = consignsaledetail =>
                consignsaledetail.ConsignSaleLineItemId == consignsaledetailId;
            var consignSaleDetail = await _consignSaleLineItemRepository.GetSingleConsignSaleLineItem(predicate);
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

            Expression<Func<MasterFashionItem, bool>> predicateMaster =
                masterItem => masterItem.MasterItemId == request.MasterItemId;
            var itemMaster = await _fashionItemRepository.GetSingleMasterItem(predicateMaster);
            if (itemMaster is null)
            {
                throw new MasterItemNotAvailableException("Master item is not found");
            }

            if (!itemMaster.Images.Select(c => c.ConsignLineItemId).Distinct().Contains(consignsaledetailId))
            {
                throw new MasterItemNotAvailableException("This master item is not allowed to use");
            }

            itemMaster.StockCount += 1;
            await _fashionItemRepository.UpdateMasterItem(itemMaster);
            var individualItem = new IndividualFashionItem()
            {
                Note = consignSaleDetail.Note,
                CreatedDate = DateTime.UtcNow,
                MasterItemId = request.MasterItemId,
                ItemCode = await _fashionItemRepository.GenerateIndividualItemCode(itemMaster.MasterItemCode),
                Status = FashionItemStatus.PendingForConsignSale,
                ConsignSaleLineItemId = consignsaledetailId,
                Condition = consignSaleDetail.Condition,

                Color = consignSaleDetail.Color,
                Size = consignSaleDetail.Size
            };
            switch (consignSaleDetail.ConsignSale.Type)
            {
                case ConsignSaleType.ForSale:
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
                        ConsignSaleLineItemId = consignsaledetailId,
                        Type = FashionItemType.ConsignedForAuction,
                        InitialPrice = consignSaleDetail.ConfirmedPrice,
                        SellingPrice = 0,
                    };
                    break;
                case ConsignSaleType.ConsignedForSale:
                    individualItem.Type = FashionItemType.ConsignedForSale;
                    individualItem.SellingPrice = consignSaleDetail.ConfirmedPrice;
                    break;
            }

            await _consignSaleLineItemRepository.UpdateConsignLineItem(consignSaleDetail);
            await _fashionItemRepository.AddInvidualFashionItem(individualItem);
            foreach (var imageRequest in consignSaleDetail.Images)
            {
                imageRequest.IndividualFashionItemId = individualItem.ItemId;
                await _imageRepository.UpdateSingleImage(imageRequest);
            }

            return new BusinessObjects.Dtos.Commons.Result<ConsignSaleLineItemResponse>()
            {
                Data = new ConsignSaleLineItemResponse()
                {
                    ConsignSaleLineItemId = consignSaleDetail.ConsignSaleLineItemId,
                    ConsignSaleLineItemStatus = consignSaleDetail.Status,
                    DealPrice = consignSaleDetail.DealPrice!.Value,
                    IsApproved = consignSaleDetail.IsApproved,
                    ResponseFromShop = consignSaleDetail.ResponseFromShop,
                    IndividualItemId = individualItem.ItemId,
                    FashionItemStatus = individualItem.Status
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
    }
}