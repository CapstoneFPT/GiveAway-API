using System.Linq.Expressions;
using AutoMapper;
using BusinessObjects.Dtos.Commons;
using BusinessObjects.Dtos.ConsignSaleDetails;
using BusinessObjects.Dtos.ConsignSales;
using BusinessObjects.Dtos.Email;
using BusinessObjects.Dtos.FashionItems;
using BusinessObjects.Entities;
using BusinessObjects.Utils;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Org.BouncyCastle.Asn1.Ocsp;
using Quartz;
using Repositories.Accounts;
using Repositories.ConsignSaleDetails;
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
        private readonly IConsignSaleDetailRepository _consignSaleDetailRepository;
        private readonly IOrderRepository _orderRepository;
        private readonly IEmailService _emailService;
        private readonly IMapper _mapper;
        private readonly ISchedulerFactory _schedulerFactory;
        private readonly IFashionItemRepository _fashionItemRepository;
        private readonly IImageRepository _imageRepository;

        public ConsignSaleService(IConsignSaleRepository consignSaleRepository, IAccountRepository accountRepository,
            IConsignSaleDetailRepository consignSaleDetailRepository
            , IOrderRepository orderRepository, IEmailService emailService, IMapper mapper,
            ISchedulerFactory schedulerFactory, IFashionItemRepository fashionItemRepository, IImageRepository imageRepository)
        {
            _consignSaleRepository = consignSaleRepository;
            _accountRepository = accountRepository;
            _consignSaleDetailRepository = consignSaleDetailRepository;
            _orderRepository = orderRepository;
            _emailService = emailService;
            _mapper = mapper;
            _schedulerFactory = schedulerFactory;
            _fashionItemRepository = fashionItemRepository;
            _imageRepository = imageRepository;
        }

        public async Task<Result<ConsignSaleResponse>> ApprovalConsignSale(Guid consignId,
            ApproveConsignSaleRequest request)
        {
            var response = new Result<ConsignSaleResponse>();
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

        public async Task<Result<ConsignSaleResponse>> ConfirmReceivedFromShop(Guid consignId)
        {
            var response = new Result<ConsignSaleResponse>();
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

        public async Task<Result<ConsignSaleResponse>> CreateConsignSale(Guid accountId,
            CreateConsignSaleRequest request)
        {
            var response = new Result<ConsignSaleResponse>();
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

        public async Task<Result<ConsignSaleResponse>> CreateConsignSaleByShop(Guid shopId,
            CreateConsignSaleByShopRequest request)
        {
            var response = new Result<ConsignSaleResponse>();
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

        public async Task<Result<PaginationResponse<ConsignSaleResponse>>> GetAllConsignSales(Guid accountId,
            ConsignSaleRequest request)
        {
            var response = new Result<PaginationResponse<ConsignSaleResponse>>();
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

        public async Task<Result<PaginationResponse<ConsignSaleResponse>>> GetAllConsignSalesByShopId(
            ConsignSaleRequestForShop request)
        {
            var response = new Result<PaginationResponse<ConsignSaleResponse>>();
            var listConsign = await _consignSaleRepository.GetAllConsignSaleByShopId(request);
            if (listConsign.TotalCount == 0)
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

        public async Task<Result<ConsignSaleResponse>> GetConsignSaleById(Guid consignId)
        {
            var response = new Result<ConsignSaleResponse>();
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

        public async Task<Result<List<ConsignSaleDetailResponse>>> GetConsignSaleDetailsByConsignSaleId(
            Guid consignsaleId)
        {
            var result = await _consignSaleDetailRepository.GetConsignSaleDetailsByConsignSaleId(consignsaleId);
            var response = new Result<List<ConsignSaleDetailResponse>>()
            {
                Data = result,
                Messages = new[] { "There are " + result.Count + " items in this consign" },
                ResultStatus = ResultStatus.Success,
            };
            return response;
        }

        public async Task<Result<MasterItemResponse>> CreateMasterItemFromConsignSaleDetail(Guid consignsaleId,
            CreateMasterItemForConsignRequest detailRequest)
        {
            var response = new Result<MasterItemResponse>();
            var consignSale =
                await _consignSaleRepository.GetSingleConsignSale(c =>
                    c.ConsignSaleId == consignsaleId);
            if (consignSale is null or not { Status: ConsignSaleStatus.AwaitDelivery})
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
                MasterItemCode = await _fashionItemRepository.GenerateConsignMasterItemCode(detailRequest.MasterItemCode,consignSale.Shop.ShopCode),
                CreatedDate = DateTime.UtcNow
            };

            await _fashionItemRepository.AddSingleMasterFashionItem(masterItem);

            /*if (consignSaleDetail.IndividualFashionItem is IndividualAuctionFashionItem auctionFashionItem)
            {
               auctionFashionItem.InitialPrice = detailRequest.SellingPrice;
               auctionFashionItem.Status = FashionItemStatus.PendingAuction;
            }
            else
            {
                consignSaleDetail.FashionItem.SellingPrice = request.SellingPrice;
            }*/
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

        public async Task<Result<ItemVariationListResponse>> CreateVariationFromConsignSaleDetail(Guid masteritemId, CreateItemVariationRequestForConsign request)
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
            return new Result<ItemVariationListResponse>()
            {
                Data = new ItemVariationListResponse()
                {
                    MasterItemId  = masteritemId,
                    CreatedDate = itemVariation.CreatedDate,
                    Condition = itemVariation.Condition,
                    Color = itemVariation.Color,
                    Size = itemVariation.Size,
                    Price = itemVariation.Price,
                    StockCount = itemVariation.StockCount,
                    VariationId = itemVariation.VariationId
                },
                ResultStatus = ResultStatus.Success,
                Messages = new []{"Create variation successfully"}
            };
        }

        public async Task<Result<FashionItemDetailResponse>> CreateIndividualItemFromConsignSaleDetail(Guid consignsaledetailId, Guid variationId,
            CreateIndividualItemRequestForConsign request)
        {
            Expression<Func<ConsignSaleDetail, bool>> predicate = consignsaledetail =>
                consignsaledetail.ConsignSaleDetailId == consignsaledetailId;
            var consignSaleDetail = await _consignSaleDetailRepository.GetSingleConsignSaleDetail(predicate);
            if (consignSaleDetail == null)
            {
                throw new ConsignSaleDetailsNotFoundException();
            }

            Expression<Func<FashionItemVariation, bool>> predicateVariation =
                itemvariation => itemvariation.VariationId == variationId;
            var itemVariation = await _fashionItemRepository.GetSingleFashionItemVariation(predicateVariation!);
            if (itemVariation is null)
            {
                throw new ItemVariationNotAvailableException("Variation is not found");
            }

            var individualItem = new IndividualFashionItem()
            {
                Note = request.Note,
                CreatedDate = DateTime.UtcNow,
                VariationId = variationId,
                ItemCode = await _fashionItemRepository.GenerateIndividualItemCode(itemVariation.MasterItem.MasterItemCode),
                Status = FashionItemStatus.PendingForConsignSale,
                ConsignSaleDetailId = consignsaledetailId
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
                        ItemCode = await _fashionItemRepository.GenerateIndividualItemCode(itemVariation.MasterItem.MasterItemCode),
                        Status = FashionItemStatus.PendingForConsignSale,
                        ConsignSaleDetailId = consignsaledetailId,
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
                    IndividualFashionItemId = individualItem.ItemId
                };
                await _imageRepository.AddImage(image);
                individualItem.Images.Add(image);
            }
            return new Result<FashionItemDetailResponse>()
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
                    CategoryName = itemVariation.MasterItem.Category.Name,
                    SellingPrice = individualItem.SellingPrice ?? 0,
                    ShopId = itemVariation.MasterItem.ShopId,
                    ShopAddress = itemVariation.MasterItem.Shop.Address
                },
                Messages = new []{"Create individual item successfully"},
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