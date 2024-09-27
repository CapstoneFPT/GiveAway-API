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
using Dao;
using DotNext;
using LinqKit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Quartz;
using Repositories.Accounts;
using Repositories.AuctionDeposits;
using Repositories.AuctionItems;
using Repositories.Auctions;
using Repositories.Bids;
using Repositories.OrderLineItems;
using Repositories.Orders;
using Repositories.Transactions;
using Repositories.Utils;
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
        private readonly IAccountRepository _accountRepository;
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public AuctionService(IAuctionRepository auctionRepository, IBidRepository bidRepository,
            IAuctionDepositRepository auctionDepositRepository, IServiceProvider serviceProvider,
            IAuctionItemRepository auctionItemRepository,
            IAccountService accountService,
            ITransactionRepository transactionRepository, IOrderRepository orderRepository,
            ISchedulerFactory schedulerFactory, IEmailService emailService, IAccountRepository accountRepository,
            IServiceScopeFactory serviceScopeFactory)
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
            _accountRepository = accountRepository;
            _serviceScopeFactory = serviceScopeFactory;
        }

        public async Task<AuctionDetailResponse> CreateAuction(CreateAuctionRequest request)
        {
            var auctionItemQuery = _auctionItemRepository.GetQueryable();

            var auctionItem = await
                auctionItemQuery
                    .Where(x => x.ItemId == request.AuctionItemId)
                    .FirstOrDefaultAsync();

            if (auctionItem == null)
            {
                throw new AuctionItemNotFoundException();
            }

            if (auctionItem.Status != FashionItemStatus.Available)
            {
                throw new AuctionItemNotAvailableForAuctioningException();
            }

            // if (await AuctionRepository.IsDateTimeOverlapped(request.StartTime, request.EndTime))
            // {
            //     throw new ScheduledTimeOverlappedException("There is an auction on time");
            // }

            if (auctionItem.InitialPrice == null)
            {
                throw new InvalidInitialPriceException("This item doesn't have an initial price");
            }


            var auction = new Auction()
            {
                EndDate = request.EndTime,
                StartDate = request.StartTime,
                Title = request.Title,
                Status = AuctionStatus.Pending,
                DepositFee = request.DepositFee,
                IndividualAuctionFashionItemId = request.AuctionItemId,
                ShopId = request.ShopId,
                CreatedDate = DateTime.UtcNow,
                StepIncrement = request.StepIncrementPercentage
            };

            var result = await _auctionRepository.CreateAuction(auction);
            await _auctionItemRepository.UpdateAuctionItemStatus(auctionItem.ItemId, FashionItemStatus.PendingAuction);
            return result;
        }

        public async Task<BusinessObjects.Dtos.Commons.Result<OrderResponse>> EndAuction(Guid id)
        {
            var auction = await _auctionRepository.GetAuction(id);
            if (auction is null)
            {
                return new BusinessObjects.Dtos.Commons.Result<OrderResponse>()
                {
                    ResultStatus = ResultStatus.NotFound,
                    Messages = new[] { "Auction Not Found" }
                };
            }

            if (auction.Status != AuctionStatus.OnGoing)
            {
                return new BusinessObjects.Dtos.Commons.Result<OrderResponse>()
                {
                    ResultStatus = ResultStatus.Error,
                    Messages = new[] { "Auction is not on going" }
                };
            }

            var winningBid = await _bidRepository.GetLargestBid(id);
            if (winningBid is null)
            {
                await _auctionRepository.UpdateAuctionStatus(auctionId: id, auctionStatus: AuctionStatus.Finished);
                return new BusinessObjects.Dtos.Commons.Result<OrderResponse>()
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
                return new BusinessObjects.Dtos.Commons.Result<OrderResponse>()
                {
                    ResultStatus = ResultStatus.Error,
                    Messages = new[] { "Failed to create order" }
                };
            }

            await _auctionRepository.UpdateAuctionStatus(auctionId: id, auctionStatus: AuctionStatus.Finished);

            return new BusinessObjects.Dtos.Commons.Result<OrderResponse>()
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

        public async Task<Result<AuctionItemDetailResponse, ErrorCode>> GetAuctionItem(Guid id)
        {
            var query = _auctionRepository.GetQueryable();
            var result = await query
                .Where(x => x.AuctionId == id)
                .Select(x => x.IndividualAuctionFashionItem)
                .Select(x => new AuctionItemDetailResponse()
                {
                    AuctionId = id,
                    Brand = x.MasterItem.Brand,
                    Status = x.Status,
                    Images = x.Images.Select(image => image.Url).ToList(),
                    CategoryName = x.MasterItem.Category.Name,
                    Color = x.Color,
                    Condition = x.Condition,
                    Description = x.MasterItem.Description ?? "N/A",
                    Gender = x.MasterItem.Gender,
                    Name = x.MasterItem.Name,
                    Note = x.Note ?? "N/A",
                    ItemCode = x.ItemCode,
                    ShopAddress = x.MasterItem.Shop.Address,
                    InitialPrice = x.InitialPrice,
                    Size = x.Size,
                    ItemId = x.ItemId,
                    SellingPrice = x.SellingPrice ?? 0,
                    FashionItemType = x.Type
                }).FirstOrDefaultAsync();

            if (result == null)
            {
                return new Result<AuctionItemDetailResponse, ErrorCode>(ErrorCode.NotFound);
            }

            return new Result<AuctionItemDetailResponse, ErrorCode>(result);
        }

        public async Task<Result<AuctionLeaderboardResponse, ErrorCode>> GetAuctionLeaderboard(Guid id,
            AuctionLeaderboardRequest request)
        {
            var auctionQuery
                = _auctionRepository.GetQueryable();
            var bidQuery = _bidRepository.GetQueryable();

            var leaderBoardQuery = bidQuery
                .Where(x => x.AuctionId == id)
                .GroupBy(x => new { x.MemberId, x.Member.Phone })
                .Select(grouping => new LeaderboardItemListResponse()
                {
                    MemberId = grouping.Key.MemberId,
                    Phone = grouping.Key.Phone,
                    HighestBid = grouping.Max(b => b.Amount), IsWon = grouping.Any(bid => bid.IsWinning)
                }).OrderByDescending(x => x.HighestBid);
            var count = await leaderBoardQuery.CountAsync();

            var data = await leaderBoardQuery
                .Skip(PaginationUtils.GetSkip(request.Page, request.PageSize))
                .Take(PaginationUtils.GetTake(request.PageSize))
                .ToListAsync();
            var paginationResponse = new PaginationResponse<LeaderboardItemListResponse>()
            {
                PageSize = request.PageSize ?? -1,
                PageNumber = request.Page ?? -1,
                TotalCount = count,
                Items = data
            };

            return new Result<AuctionLeaderboardResponse, ErrorCode>(
                new AuctionLeaderboardResponse()
                {
                    AuctionId = id,
                    Leaderboard = paginationResponse
                });
        }

        public async Task<PaginationResponse<AuctionListResponse>> GetAuctionList(GetAuctionsRequest request)
        {
            Expression<Func<Auction, bool>> predicate = auction => true;

            if (!request.GetExpiredAuctions)
            {
                predicate = predicate.And(auction => auction.EndDate >= DateTime.UtcNow);
            }

            if (!string.IsNullOrEmpty(request.Title))
            {
                predicate = predicate.And(auction => EF.Functions.ILike(auction.Title, $"%{request.Title}%"));
            }

            if (!string.IsNullOrEmpty(request.AuctionCode))
            {
                predicate = predicate.And(
                    auction => EF.Functions.ILike(auction.AuctionCode, $"%{request.AuctionCode}%"));
            }

            if (request.Statuses.Length > 0)
            {
                predicate = predicate.And(auction => request.Statuses.Contains(auction.Status));
            }

            Expression<Func<Auction, AuctionListResponse>> selector = auction => new AuctionListResponse()
            {
                AuctionId = auction.AuctionId,
                Title = auction.Title,
                StartDate = auction.StartDate,
                EndDate = auction.EndDate,
                Status = auction.Status,
                DepositFee = auction.DepositFee,
                InitialPrice = auction.IndividualAuctionFashionItem.InitialPrice,
                ImageUrl = auction.IndividualAuctionFashionItem.Images.FirstOrDefault() != null
                    ? auction.IndividualAuctionFashionItem.Images.FirstOrDefault().Url
                    : null,
                AuctionCode = auction.AuctionCode,
                SucessfulBidAmount = auction.Bids.Where(c => c.IsWinning == true).Sum(c => c.Amount),
                IsWon = auction.Bids.Any(),
                ItemCode = auction.IndividualAuctionFashionItem.ItemCode,
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

        public async Task<AuctionDetailResponse?> GetAuction(Guid id, Guid? memberId)
        {
            var queryable = _auctionRepository.GetQueryable();

            var result = await queryable
                .Include(x => x.IndividualAuctionFashionItem)
                .Include(x => x.Shop)
                .Where(x => x.AuctionId == id)
                .Select(x => new AuctionDetailResponse()
                {
                    AuctionId = x.AuctionId,
                    Title = x.Title,
                    StartDate = x.StartDate,
                    EndDate = x.EndDate,
                    Status = x.Status,
                    CreatedDate = x.CreatedDate,
                    IndividualItemCode = x.IndividualAuctionFashionItem.ItemCode,
                    ShopAddress = x.Shop.Address,
                    AuctionCode = x.AuctionCode,
                    DepositFee = x.DepositFee,
                    StepIncrement = x.StepIncrement,
                    Won = x.Bids.Where(bid => bid.MemberId == memberId).Any(bid => bid.IsWinning == true),
                })
                .FirstOrDefaultAsync();

            if (result == null)
            {
                throw new AuctionNotFoundException();
            }

            return result;
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
            using var scope = _serviceScopeFactory.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<GiveAwayDbContext>();

            using var dbContextTransaction = await dbContext.Database.BeginTransactionAsync();

            try
            {
                var auction = await _auctionRepository.GetAuction(auctionId, true);
                var admin = await _accountRepository.FindOne(account => account.Role == Roles.Admin);
                var existingDeposit =
                    await _auctionDepositRepository.GetSingleDeposit<AuctionDeposit>(
                        x => x.AuctionId == auctionId && x.MemberId == request.MemberId, null);

                if (existingDeposit != null)
                {
                    throw new InvalidOperationException("Member has already placed a deposit");
                }

                if (auction is null)
                {
                    throw new AuctionNotFoundException();
                }

                if (admin == null)
                {
                    throw new AccountNotFoundException();
                }

                var member = await _accountRepository.FindOne(account => account.AccountId == request.MemberId);

                if (member.Balance < auction.DepositFee)
                {
                    throw new BalanceIsNotEnoughException(ErrorCode.PaymentFailed);
                }
                if (member is null)
                {
                    throw new AccountNotFoundException();
                }

                if (auction.IndividualAuctionFashionItem.ConsignSaleLineItem!.ConsignSale.MemberId == request.MemberId)
                {
                    throw new NotAllowToPlaceDeposit(
                        "This product is your consign product so you are not allowed to participate auction");
                }

                var deposit = new AuctionDeposit()
                {
                    AuctionId = auctionId,
                    MemberId = request.MemberId,
                    CreatedDate = DateTime.UtcNow,
                };

                var createDepositResult = await _auctionDepositRepository.CreateAuctionDeposit(deposit);


                member.Balance -= auction.DepositFee;
                admin.Balance -= auction.DepositFee;
                await _accountRepository.UpdateAccount(admin);
                await _accountRepository.UpdateAccount(member);


                var transaction = new Transaction()
                {
                    Amount = auction.DepositFee,
                    Type = TransactionType.AuctionDeposit,
                    SenderId = request.MemberId,
                    SenderBalance = member.Balance,
                    ReceiverId = admin.AccountId,
                    ReceiverBalance = admin.Balance,
                    AuctionDepositId = createDepositResult.AuctionDepositId,
                    CreatedDate = DateTime.UtcNow,
                    PaymentMethod = PaymentMethod.Point,
                    VnPayTransactionNumber = "N/A"
                };
                var transactionResult = await _transactionRepository.CreateTransaction(transaction);

                if (transactionResult != null)
                {
                    await _emailService.SendEmailAuctionIsComing(auctionId, request.MemberId);
                    return new AuctionDepositDetailResponse()
                    {
                        Amount = auction.DepositFee,
                        CreatedDate = transaction.CreatedDate,
                        AuctionId = auctionId,
                        DepositCode = createDepositResult.DepositCode,
                        MemberId = request.MemberId,
                        Id = createDepositResult.AuctionDepositId,
                    };
                }

                throw new TransactionFailedException();
            }
            catch (Exception e)
            {
                await dbContextTransaction.RollbackAsync();
                throw new TransactionFailedException();
            }
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

            await ScheduleAuctionStart(result);
            await ScheduleAuctionEnd(result);
            return result;
        }

        private async Task ScheduleAuctionStart(AuctionDetailResponse auction)
        {
            var scheduler = await _schedulerFactory.GetScheduler();
            var jobDataMap = new JobDataMap()
            {
                { "AuctionStartId", auction.AuctionId }
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
        }

        private async Task ScheduleAuctionEnd(AuctionDetailResponse auction)
        {
            var scheduler = await _schedulerFactory.GetScheduler();
            var jobDataMap = new JobDataMap()
            {
                { "AuctionEndId", auction.AuctionId }
            };

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


        public async Task<PaginationResponse<BidListResponse>?> GetBids(Guid id, GetBidsRequest request)
        {
            var query = _bidRepository.GetQueryable();

            Expression<Func<Bid, bool>> predicate = bid => bid.AuctionId == id;

            if (request.MemberId != null)
            {
                predicate = predicate.And(bid => bid.MemberId == request.MemberId);
            }

            if (request.Phone != null)
            {
                predicate = predicate.And(bid => bid.Member.Phone.Contains(request.Phone));
            }

            if (request.MemberName != null)
            {
                predicate = predicate.And(bid =>
                    bid != null && EF.Functions.ILike(bid.Member.Fullname, $"%{request.MemberName}%"));
            }
            var count = await query.Where(predicate).CountAsync();

            var items = await query
                .Where(predicate)
                .OrderByDescending(bid => bid.CreatedDate)
                .Skip(PaginationUtils.GetSkip(request.PageNumber, request.PageSize))
                .Take(PaginationUtils.GetTake(request.PageSize))
                .Select(bid => new BidListResponse()
                {
                    AuctionId = bid.AuctionId,
                    MemberId = bid.MemberId,
                    Amount = bid.Amount,
                    Phone = bid.Member.Phone,
                    MemberName = bid.Member.Fullname,
                    CreatedDate = bid.CreatedDate,
                    IsWinning = bid.IsWinning,
                    BidCode = bid.BidCode,
                    Id = bid.BidId
                }).ToListAsync();

            return new PaginationResponse<BidListResponse>()
            {
                Items = items,
                PageNumber = request.PageNumber ?? -1,
                PageSize = request.PageSize ?? -1,
                TotalCount = count
            };
        }

        public Task<BidDetailResponse?> GetLargestBid(Guid auctionId)
        {
            var result = _bidRepository.GetLargestBid(auctionId);
            return result;
        }
    }
}