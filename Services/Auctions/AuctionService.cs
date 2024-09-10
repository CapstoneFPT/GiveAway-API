using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using BusinessObjects.Dtos.AuctionDeposits;
using BusinessObjects.Dtos.Auctions;
using BusinessObjects.Dtos.Bids;
using BusinessObjects.Dtos.Commons;
using BusinessObjects.Dtos.Orders;
using BusinessObjects.Entities;
using BusinessObjects.Utils;
using LinqKit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Quartz;
using Repositories.AuctionDeposits;
using Repositories.AuctionItems;
using Repositories.Auctions;
using Repositories.Bids;
using Repositories.OrderLineItems;
using Repositories.Orders;
using Repositories.Transactions;
using Services.Accounts;
using Services.Emails;
using Services.Orders;
using Services.Transactions;

namespace Services.Auctions
{
    public class AuctionService : IAuctionService
    {
        private readonly IAuctionRepository _auctionRepository;
        private readonly IBidRepository _bidRepository;
        private readonly IAuctionDepositRepository _auctionDepositRepository;
        private readonly IServiceProvider _serviceProvider;
        private readonly IAuctionItemRepository _auctionItemRepository;
        private readonly IAccountService _accountService;
        private readonly ITransactionRepository _transactionRepository;
        private readonly IOrderRepository _orderRepository;
        private readonly ISchedulerFactory _schedulerFactory;
        private readonly IEmailService _emailService;

        public AuctionService(IAuctionRepository auctionRepository, IBidRepository bidRepository,
            IAuctionDepositRepository auctionDepositRepository, IServiceProvider serviceProvider,
            IAuctionItemRepository auctionItemRepository,
            IAccountService accountService,
            ITransactionRepository transactionRepository, IOrderRepository orderRepository,
            ISchedulerFactory schedulerFactory, IEmailService emailService)
        {
            _auctionRepository = auctionRepository;
            _bidRepository = bidRepository;
            _auctionDepositRepository = auctionDepositRepository;
            _serviceProvider = serviceProvider;
            _auctionItemRepository = auctionItemRepository;
            _accountService = accountService;
            _transactionRepository = transactionRepository;
            _orderRepository = orderRepository;
            _schedulerFactory = schedulerFactory;
            _emailService = emailService;
        }

        public async Task<AuctionDetailResponse> CreateAuction(CreateAuctionRequest request)
        {
            var result = await _auctionRepository.CreateAuction(request);
            return result;
        }

        public async Task<Result<OrderResponse>> EndAuction(Guid id)
        {
            var auction = await _auctionRepository.GetAuction(id);
            if (auction is null)
            {
                return new Result<OrderResponse>()
                {
                    ResultStatus = ResultStatus.NotFound,
                    Messages = new[] { "Auction Not Found" }
                };
            }

            if (auction.Status != AuctionStatus.OnGoing)
            {
                return new Result<OrderResponse>()
                {
                    ResultStatus = ResultStatus.Error,
                    Messages = new[] { "Auction is not on going" }
                };
            }

            var winningBid = await _bidRepository.GetLargestBid(id);
            if (winningBid is null)
            {
                await _auctionRepository.UpdateAuctionStatus(auctionId: id, auctionStatus: AuctionStatus.Finished);
                return new Result<OrderResponse>()
                {
                    ResultStatus = ResultStatus.Success, Messages = new[] { "No Bids" }
                };
            }

            using var scope = _serviceProvider.CreateScope();
            var orderService = scope.ServiceProvider.GetRequiredService<IOrderService>();

            var createOrderRequest = new CreateOrderFromBidRequest()
            {
                MemberId = winningBid.MemberId,
                OrderCode = _orderRepository.GenerateUniqueString(),
                PaymentMethod = PaymentMethod.Point,
                TotalPrice = winningBid.Amount,
                BidId = winningBid.Id,
                AuctionFashionItemId = auction.IndividualAuctionFashionItemId
            };


            var orderResult = await orderService.CreateOrderFromBid(createOrderRequest);

            if (orderResult.ResultStatus != ResultStatus.Success)
            {
                return new Result<OrderResponse>()
                {
                    ResultStatus = ResultStatus.Error,
                    Messages = new[] { "Failed to create order" }
                };
            }

            await _auctionRepository.UpdateAuctionStatus(auctionId: id, auctionStatus: AuctionStatus.Finished);

            return new Result<OrderResponse>()
            {
                ResultStatus = ResultStatus.Success,
                Messages = new[] { "Auction Ended Successfully and Order Created" }, Data = orderResult.Data
            };
        }

        public async Task StartAuction(Guid auctionId)
        {
            var auctionUpdateResult =
                await _auctionRepository.UpdateAuctionStatus(auctionId, AuctionStatus.OnGoing);

            var auctionFashionItemId = auctionUpdateResult
                .IndividualAuctionFashionItemId;

            await _auctionItemRepository
                .UpdateAuctionItemStatus(auctionFashionItemId, FashionItemStatus.Bidding);
        }

        public Task<PaginationResponse<AuctionDepositListResponse>> GetAuctionDeposits(Guid auctionId,
            GetDepositsRequest request)
        {
            var result = _auctionDepositRepository.GetAuctionDeposits(auctionId, request);
            return result;
        }

        public async Task<PaginationResponse<AuctionListResponse>> GetAuctionList(GetAuctionsRequest request)
        {
            Expression<Func<Auction, bool>> predicate = auction => true;

            if (!request.GetExpiredAuctions)
            {
                predicate = auction => auction.EndDate >= DateTime.UtcNow;
            }

            if (request.SearchTerm is not null)
            {
                predicate = predicate.And(auction => EF.Functions.ILike(auction.Title, $"%{request.SearchTerm}%"));
            }

            if (request.Status.Length > 0)
            {
                predicate = predicate.And(auction => request.Status.Contains(auction.Status));
            }
            
            Expression<Func<Auction, AuctionListResponse>> selector = auction => new AuctionListResponse()
            {
                AuctionId = auction.AuctionId,
                Title = auction.Title,
                StartDate = auction.StartDate,
                EndDate = auction.EndDate,
                Status = auction.Status,
                DepositFee = auction.DepositFee,
                ImageUrl = auction.IndividualAuctionFashionItem.Images.FirstOrDefault().Url,
                AuctionItemId = auction.IndividualAuctionFashionItemId,
                ShopId = auction.ShopId
            };

            (List<AuctionListResponse> Items, int Page, int PageSize, int Total) result =
                await _auctionRepository.GetAuctionProjections<AuctionListResponse>(request.PageNumber,
                    request.PageSize, predicate, selector);
            return new PaginationResponse<AuctionListResponse>()
            {
                Items = result.Items,
                PageNumber = result.Page,
                PageSize = result.PageSize,
                TotalCount = result.Total,
            };
        }

        public async Task<AuctionDetailResponse?> GetAuction(Guid id)
        {
            var result = await _auctionRepository.GetAuction(id, true);

            if (result == null)
            {
                throw new AuctionNotFoundException();
            }

            return new AuctionDetailResponse()
            {
                AuctionId = result.AuctionId,
                Title = result.Title,
                StartDate = result.StartDate,
                EndDate = result.EndDate,
                Status = result.Status,
                DepositFee = result.DepositFee,
                StepIncrement = result.StepIncrement,
                AuctionItem = new AuctionItemDetailResponse()
                {
                    ItemId = result.IndividualAuctionFashionItemId,
                    Name = result.IndividualAuctionFashionItem.MasterItem.Name,
                    FashionItemType = result.IndividualAuctionFashionItem.Type,
                    SellingPrice = result.IndividualAuctionFashionItem.SellingPrice ?? 0,
                    InitialPrice = result.IndividualAuctionFashionItem.InitialPrice,
                    Size = result.IndividualAuctionFashionItem.Size,
                    Color = result.IndividualAuctionFashionItem.Color,
                    Gender = result.IndividualAuctionFashionItem.MasterItem.Gender,
                    Description = result.IndividualAuctionFashionItem.MasterItem.Description,
                    Brand = result.IndividualAuctionFashionItem.MasterItem.Brand ?? "N/A",
                    Condition = result.IndividualAuctionFashionItem.Condition ?? "N/A",
                    Note = result.IndividualAuctionFashionItem.Note,
                    Category = new AuctionItemCategory()
                    {
                        CategoryId = result.IndividualAuctionFashionItem.MasterItem.CategoryId,
                        CategoryName = result.IndividualAuctionFashionItem.MasterItem.Category.Name,
                        Level = result.IndividualAuctionFashionItem.MasterItem.Category.Level
                    },
                    Shop = new ShopAuctionDetailResponse()
                    {
                        ShopId = result.Shop.ShopId,
                        Address = result.Shop.Address,
                    },
                    Images = result.IndividualAuctionFashionItem.Images.Count > 0 ? result.IndividualAuctionFashionItem.Images.Select(
                        img => new FashionItemImage()
                        {
                            ImageId = img.ImageId,
                            ImageUrl = img.Url
                        }).ToList() : []
                }
            };
        }

        public Task<AuctionDetailResponse?> DeleteAuction(Guid id)
        {
            var result = _auctionRepository.DeleteAuction(id);
            return result;
        }

        public Task<AuctionDetailResponse> UpdateAuction(Guid id, UpdateAuctionRequest request)
        {
            var result = _auctionRepository.UpdateAuction(id, request);
            return result;
        }

        public async Task<AuctionDepositDetailResponse> PlaceDeposit(Guid auctionId,
            CreateAuctionDepositRequest request)
        {
            var auction = await _auctionRepository.GetAuction(auctionId);

            if (auction is null)
            {
                throw new AuctionNotFoundException();
            }

            await _accountService.DeductPoints(request.MemberId, auction.DepositFee);
            var transaction = new Transaction()
            {
                Amount = auction.DepositFee,
                Type = TransactionType.AuctionDeposit,
                MemberId = request.MemberId,
                CreatedDate = DateTime.UtcNow,
                VnPayTransactionNumber = "N/A"
            };
            var transactionResult = await _transactionRepository.CreateTransaction(transaction);

            var result = await _auctionDepositRepository.CreateDeposit(auctionId, request,transactionResult.TransactionId);
            await _emailService.SendEmailAuctionIsComing(auctionId, request.MemberId);
            return result;
        }

        public async Task<AuctionDepositDetailResponse?> GetDeposit(Guid id, Guid depositId)
        {
            Expression<Func<AuctionDeposit, bool>> predicate = deposit => deposit.AuctionDepositId == depositId;
            Expression<Func<AuctionDeposit, AuctionDepositDetailResponse>> selector = deposit =>
                new AuctionDepositDetailResponse()
                {
                    Id = deposit.AuctionDepositId,
                    AuctionId = deposit.AuctionId,
                    MemberId = deposit.MemberId,
                    Amount = deposit.Auction.DepositFee,
                    CreatedDate = deposit.CreatedDate,
                    TransactionId = deposit.TransactionId,
                };
            var result =
                await _auctionDepositRepository.GetSingleDeposit<AuctionDepositDetailResponse>(predicate, selector);
            return result;
        }

        public async Task<AuctionDetailResponse?> ApproveAuction(Guid id)
        {
            var result = await _auctionRepository.ApproveAuction(id);

            if (result == null)
            {
                throw new AuctionNotFoundException();
            }

            await ScheduleAuctionStartAndEnd(result);
            return result;
        }

        private async Task ScheduleAuctionStartAndEnd(AuctionDetailResponse auction)
        {
            var scheduler = await _schedulerFactory.GetScheduler();
            var jobDataMap = new JobDataMap()
            {
                { "AuctionId", auction.AuctionId }
            };

            var startJob = JobBuilder.Create<AuctionStartingJob>()
                .WithIdentity($"StartAuction_{auction.AuctionId}")
                .SetJobData(jobDataMap)
                .Build();

            var startTrigger = TriggerBuilder.Create()
                .WithIdentity($"StartAuctionTrigger_{auction.AuctionId}")
                .StartAt(new DateTimeOffset(auction.StartDate))
                .Build();

            await scheduler.ScheduleJob(startJob, startTrigger);

            var endJob = JobBuilder.Create<AuctionEndingJob>()
                .WithIdentity($"EndAuction_{auction.AuctionId}")
                .SetJobData(jobDataMap)
                .Build();

            var endTrigger = TriggerBuilder.Create()
                .WithIdentity($"EndAuctionTrigger_{auction.AuctionId}")
                .StartAt(new DateTimeOffset(auction.EndDate))
                .Build();

            await scheduler.ScheduleJob(endJob, endTrigger);
        }

        public Task<RejectAuctionResponse?> RejectAuction(Guid id)
        {
            var result = _auctionRepository.RejectAuction(id);
            return result;
        }

        public async Task<BidDetailResponse?> PlaceBid(Guid id, CreateBidRequest request)
        {
            var result = await _bidRepository.CreateBid(id, request);
            return result;
        }


        public Task<PaginationResponse<BidListResponse>?> GetBids(Guid id, GetBidsRequest request)
        {
            var result = _bidRepository.GetBids(id, request);
            return result;
        }

        public Task<BidDetailResponse?> GetLargestBid(Guid auctionId)
        {
            var result = _bidRepository.GetLargestBid(auctionId);
            return result;
        }
    }
}