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

        public async Task<BusinessObjects.Dtos.Commons.Result<ConsignSaleResponse>> ApprovalConsignSale(Guid consignId,
            ApproveConsignSaleRequest request)
        {
            var response = new BusinessObjects.Dtos.Commons.Result<ConsignSaleResponse>();
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

            response.Data = await _consignSaleRepository.ApprovalConsignSale(consignId, request.Status);
            await _emailService.SendEmailConsignSale(consignId);
            response.Messages = ["Approval successfully"];
            response.ResultStatus = ResultStatus.Success;
            return response;
        }

        public async Task<BusinessObjects.Dtos.Commons.Result<ConsignSaleResponse>> ConfirmReceivedFromShop(
            Guid consignId)
        {
            var response = new BusinessObjects.Dtos.Commons.Result<ConsignSaleResponse>();
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
            await ScheduleConsignEnding(result);
            await _emailService.SendEmailConsignSaleReceived(consignId);
            response.Data = result;
            response.Messages = ["Confirm received successfully"];
            response.ResultStatus = ResultStatus.Success;
            return response;
        }

        private async Task ScheduleConsignEnding(ConsignSaleResponse consign)
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
                .StartAt(new DateTimeOffset(consign.EndDate.Value))
                .Build();
            await schedule.ScheduleJob(endJob, endTrigger);
        }

        public async Task<BusinessObjects.Dtos.Commons.Result<ConsignSaleResponse>> CreateConsignSale(Guid accountId,
            CreateConsignSaleRequest request)
        {
            var response = new BusinessObjects.Dtos.Commons.Result<ConsignSaleResponse>();
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

        public async Task<BusinessObjects.Dtos.Commons.Result<ConsignSaleResponse>> CreateConsignSaleByShop(Guid shopId,
            CreateConsignSaleByShopRequest request)
        {
            var response = new BusinessObjects.Dtos.Commons.Result<ConsignSaleResponse>();
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

            await ScheduleConsignEnding(consign);
            response.Data = consign;
            response.ResultStatus = ResultStatus.Success;
            response.Messages = ["Create successfully"];
            return response;
        }

        public async Task<BusinessObjects.Dtos.Commons.Result<PaginationResponse<ConsignSaleResponse>>>
            GetAllConsignSales(Guid accountId,
                ConsignSaleRequest request)
        {
            var response = new BusinessObjects.Dtos.Commons.Result<PaginationResponse<ConsignSaleResponse>>();
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
                predicate = predicate.And(x=> EF.Functions.ILike(x.ConsignSaleCode, $"%{request.ConsignSaleCode}%"));
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

            if (request.Email !=null)
            {
               predicate = predicate.And(x => x.Email != null && EF.Functions.ILike(x.Email , $"%{request.Email}%")); 
            }

            if (request.ConsignType != null)
            {
               predicate = predicate.And(x => x.Type == request.ConsignType); 
            }

            if (request.ConsignorName != null)
            {
                predicate = predicate.And(x=>x.ConsignorName != null && EF.Functions.ILike(x.ConsignorName , $"%{request.ConsignorName}%"));
            }

            if (request.ConsignorPhone != null)
            {
                predicate = predicate.And(x=>EF.Functions.Like(x.Phone , $"%{request.ConsignorPhone}%"));
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
                return new Result<PaginationResponse<ConsignSaleListResponse>,ErrorCode>(ErrorCode.ServerError);
            }
        }

      

        public async Task<BusinessObjects.Dtos.Commons.Result<ConsignSaleResponse>> GetConsignSaleById(Guid consignId)
        {
            var response = new BusinessObjects.Dtos.Commons.Result<ConsignSaleResponse>();
            var Consign = await _consignSaleRepository.GetConsignSaleById(consignId);
            if (Consign == null)
            {
                response.Messages = ["Consignment is not found"];
                response.ResultStatus = ResultStatus.Error;
                return response;
            }

            response.Data = Consign;
            response.Messages = ["Successfully"];
            response.ResultStatus = ResultStatus.Success;
            return response;
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
                    ProductName = lineItem.ProductName,
                    Condition = lineItem.Condition,
                    Brand = lineItem.Brand,
                    Color = lineItem.Color,
                    Gender = lineItem.Gender,
                    Size = lineItem.Size,
                    Images = lineItem.Images.Select(x => x.Url ?? string.Empty).ToList(),
                    ConfirmedPrice = lineItem.ConfirmedPrice,
                    Note = lineItem.Note,
                    DealPrice = lineItem.DealPrice
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
            CreateMasterItemFromConsignSaleLineItem(Guid consignsaleId,
                CreateMasterItemForConsignRequest detailRequest)
        {
            var response = new BusinessObjects.Dtos.Commons.Result<MasterItemResponse>();
            var consignSale =
                await _consignSaleRepository.GetSingleConsignSale(c =>
                    c.ConsignSaleId == consignsaleId);
            if (consignSale is null or not { Status: ConsignSaleStatus.AwaitDelivery })
            {
                throw new ConsignSaleNotFoundException();
            }

            var masterItem = new MasterFashionItem()
            {
                Brand = detailRequest.Brand,
                Description = detailRequest.Description,
                Name = detailRequest.Name,
                IsConsignment = true,
                Gender = detailRequest.Gender,
                ShopId = consignSale.ShopId,
                CategoryId = detailRequest.CategoryId,
                MasterItemCode =
                    await _fashionItemRepository.GenerateConsignMasterItemCode(detailRequest.MasterItemCode,
                        consignSale.Shop.ShopCode),
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

        public async Task<BusinessObjects.Dtos.Commons.Result<ItemVariationListResponse>>
            CreateVariationFromConsignSaleLineItem(Guid masteritemId, CreateItemVariationRequestForConsign request)
        {
            Expression<Func<MasterFashionItem, bool>> predicate = masterItem => masterItem.MasterItemId == masteritemId;
            var masterItem = await _fashionItemRepository.GetSingleMasterItem(predicate);
            if (masterItem == null || masterItem.IsConsignment == false)
            {
                throw new MasterItemNotAvailableException(
                    "This master item is not available to create item for consign");
            }

            var itemVariation = new FashionItemVariation()
            {
                MasterItemId = masteritemId,
                CreatedDate = DateTime.UtcNow,
                Condition = request.Condition,
                Color = request.Color,
                Size = request.Size,
                Price = request.Price,
                StockCount = 0
            };
            await _fashionItemRepository.AddSingleFashionItemVariation(itemVariation);
            return new BusinessObjects.Dtos.Commons.Result<ItemVariationListResponse>()
            {
                Data = new ItemVariationListResponse()
                {
                    MasterItemId = masteritemId,
                    CreatedDate = itemVariation.CreatedDate,
                    Condition = itemVariation.Condition,
                    Color = itemVariation.Color,
                    Size = itemVariation.Size,
                    Price = itemVariation.Price,
                    StockCount = itemVariation.StockCount,
                    VariationId = itemVariation.VariationId
                },
                ResultStatus = ResultStatus.Success,
                Messages = new[] { "Create variation successfully" }
            };
        }

        public async Task<BusinessObjects.Dtos.Commons.Result<FashionItemDetailResponse>>
            CreateIndividualItemFromConsignSaleLineItem(Guid consignsaledetailId, Guid variationId,
                CreateIndividualItemRequestForConsign request)
        {
            Expression<Func<ConsignSaleLineItem, bool>> predicate = consignsaledetail =>
                consignsaledetail.ConsignSaleLineItemId == consignsaledetailId;
            var consignSaleDetail = await _consignSaleLineItemRepository.GetSingleConsignSaleLineItem(predicate);
            if (consignSaleDetail == null)
            {
                throw new ConsignSaleDetailsNotFoundException();
            }

            consignSaleDetail.ConfirmedPrice = request.ConfirmPrice;
            await _consignSaleLineItemRepository.UpdateConsignLineItem(consignSaleDetail);
            Expression<Func<FashionItemVariation, bool>> predicateVariation =
                itemvariation => itemvariation.VariationId == variationId;
            var itemVariation = await _fashionItemRepository.GetSingleFashionItemVariation(predicateVariation!);
            if (itemVariation is null)
            {
                throw new ItemVariationNotAvailableException("Variation is not found");
            }

            itemVariation.StockCount += 1;
            await _fashionItemRepository.UpdateFashionItemVariation(itemVariation);
            var individualItem = new IndividualFashionItem()
            {
                Note = request.Note,
                CreatedDate = DateTime.UtcNow,
                VariationId = variationId,
                ItemCode = await _fashionItemRepository.GenerateIndividualItemCode(itemVariation.MasterItem
                    .MasterItemCode),
                Status = FashionItemStatus.PendingForConsignSale,
                ConsignSaleLineItemId = consignsaledetailId
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
                        Note = request.Note,
                        CreatedDate = DateTime.UtcNow,
                        VariationId = variationId,
                        ItemCode = await _fashionItemRepository.GenerateIndividualItemCode(itemVariation.MasterItem
                            .MasterItemCode),
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

            await _fashionItemRepository.AddInvidualFashionItem(individualItem);
            foreach (var imageRequest in request.Images)
            {
                var image = new Image()
                {
                    Url = imageRequest,
                    CreatedDate = DateTime.UtcNow,
                    IndividualFashionItemId = individualItem.ItemId,
                    ConsignLineItemId = consignsaledetailId
                };
                await _imageRepository.AddImage(image);
                individualItem.Images.Add(image);
            }

            return new BusinessObjects.Dtos.Commons.Result<FashionItemDetailResponse>()
            {
                Data = new FashionItemDetailResponse()
                {
                    Type = individualItem.Type,
                    Description = itemVariation.MasterItem.Description ?? string.Empty,
                    Brand = itemVariation.MasterItem.Brand,
                    Gender = itemVariation.MasterItem.Gender,
                    Name = itemVariation.MasterItem.Name,
                    IsConsignment = itemVariation.MasterItem.IsConsignment,
                    Color = itemVariation.Color,
                    Size = itemVariation.Size,
                    Condition = itemVariation.Condition,
                    ItemCode = individualItem.ItemCode,
                    Note = individualItem.Note,
                    Status = individualItem.Status,
                    Images = individualItem.Images.Select(c => c.Url).ToList(),
                    ItemId = individualItem.ItemId, 
                    CategoryId = itemVariation.MasterItem.CategoryId,
                    // CategoryName = itemVariation.MasterItem.Category.Name,
                    SellingPrice = individualItem.SellingPrice ?? 0,
                    ShopId = itemVariation.MasterItem.ShopId,
                    // ShopAddress = itemVariation.MasterItem.Shop.Address,
                    // IsOrderedYet = false,
                },
                Messages = new[] { "Create individual item successfully" },
                ResultStatus = ResultStatus.Success
            };
        }

        public async Task UpdateConsignPrice(Guid orderId)
        {
            var order = await _orderRepository.GetSingleOrder(c => c.OrderId == orderId);
            foreach (var detail in order.OrderDetails)
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