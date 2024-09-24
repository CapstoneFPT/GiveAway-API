using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessObjects.Dtos.Bids;
using BusinessObjects.Dtos.Commons;
using BusinessObjects.Entities;
using BusinessObjects.Utils;
using Dao;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Repositories.Bids
{
    public class BidRepository : IBidRepository
    {
        private const string Prefix = "BID";
        private static Random _random = new();
        private readonly ILogger<BidRepository> _logger;
        private readonly GiveAwayDbContext _giveAwayDbContext;

        public BidRepository(GiveAwayDbContext giveAwayDbContext, ILogger<BidRepository> logger)
        {
            _giveAwayDbContext = giveAwayDbContext;
            _logger = logger;
        }

        public async Task<string> GenerateUniqueString()
        {
            for (int attempt = 0; attempt < 5; attempt++)
            {
                string code = GenerateCode();
                bool isCodeExisted = await _giveAwayDbContext.Recharges.AnyAsync(r => r.RechargeCode == code);

                if (!isCodeExisted)
                {
                    return code;
                }

                await Task.Delay(100 * (int)Math.Pow(2, attempt));
            }

            throw new Exception("Failed to generate unique code after multiple attempts");
        }

        private static string GenerateCode()
        {
            string timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
            string randomString = _random.Next(1000, 9999).ToString();
            return $"{Prefix}-{timestamp}-{randomString}";
        }

        public async Task<BidDetailResponse?> CreateBid(Guid id, CreateBidRequest request)
        {
            var auction = await GenericDao<Auction>.Instance.GetQueryable().Include(x => x.IndividualAuctionFashionItem)
                .FirstOrDefaultAsync(x => x.AuctionId == id);

            var phoneNumber = await _giveAwayDbContext.Accounts
                .Select(x => new
                {
                    Phone = x.Phone,
                    AccountId = x.AccountId
                })
                .FirstOrDefaultAsync(x => x.AccountId == request.MemberId);
            
            _logger.LogInformation(
                "CreateBid: {AuctionId}, {MemberId}, {Amount}, {PhoneNumber}",
                id,
                request.MemberId,
                request.Amount,
                phoneNumber.Phone
                );
            
            if (auction == null)
            {
                throw new AuctionNotFoundException();
            }

            if (auction.EndDate <= DateTime.UtcNow || auction.Status == AuctionStatus.Finished)
            {
                throw new InvalidOperationException("Auction not found or have ended");
            }

            if (auction.StartDate > DateTime.UtcNow || auction.Status != AuctionStatus.OnGoing)
            {
                throw new InvalidOperationException("Auction has not started yet");
            }

            var auctionDeposit = await GenericDao<AuctionDeposit>.Instance.GetQueryable()
                .FirstOrDefaultAsync(x => x.AuctionId == id && x.MemberId == request.MemberId);

            if (auctionDeposit == null)
            {
                throw new AuctionDepositNotFoundException();
            }

            var latestBid = await GenericDao<Bid>.Instance.GetQueryable()
                .OrderByDescending(x => x.CreatedDate)
                .FirstOrDefaultAsync(x => x.AuctionId == id);

            if (latestBid != null && request.MemberId == latestBid.MemberId)
            {
                throw new InvalidOperationException("You have already bid on this auction");
            }

            var currentBidRequired = latestBid != null
                ? latestBid.Amount + auction.StepIncrement
                : auction.IndividualAuctionFashionItem.InitialPrice;

            if (request.Amount < currentBidRequired)
            {
                throw new InvalidOperationException("Bid amount must be greater than previous bid");
            }

            var newBid = new Bid
            {
                Amount = request.Amount,
                MemberId = request.MemberId,
                AuctionId = id,
                IsWinning = true,
                BidCode = await GenerateUniqueString(),
                CreatedDate = DateTime.UtcNow
            };

            var result = await GenericDao<Bid>.Instance.AddAsync(newBid);

            if (latestBid != null)
            {
                latestBid.IsWinning = false;
                await GenericDao<Bid>.Instance.UpdateAsync(latestBid);
            }

            return new BidDetailResponse
            {
                AuctionId = id,
                Amount = result.Amount,
                MemberId = result.MemberId,
                Id = result.BidId,
                IsWinning = true,
                Phone = phoneNumber.Phone,
                CreatedDate = result.CreatedDate,
                NextAmount = result.Amount + auction.StepIncrement
            };
        }


        public async Task<BidDetailResponse?> GetLargestBid(Guid auctionId)
        {
            var result = await GenericDao<Bid>.Instance.GetQueryable()
                    .OrderByDescending(x => x.Amount)
                    .Select(x => new BidDetailResponse()
                    {
                        Amount = x.Amount,
                        AuctionId = x.AuctionId,
                        Id = x.BidId,
                        MemberId = x.MemberId,
                        CreatedDate = x.CreatedDate,
                        IsWinning = x.IsWinning,
                        NextAmount = x.Amount + x.Auction.StepIncrement
                    })
                    .FirstOrDefaultAsync(x => x.AuctionId == auctionId)
                ;

            return result;
        }

        public IQueryable<Bid> GetQueryable()
        {
            return _giveAwayDbContext.Bids.AsQueryable();
        }
    }
}