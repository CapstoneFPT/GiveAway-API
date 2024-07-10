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

namespace Repositories.Bids
{
    public class BidRepository : IBidRepository
    {
        private readonly GenericDao<Auction> _auctionDao;
        private readonly GenericDao<Bid> _bidDao;
        private readonly GenericDao<AuctionDeposit> _auctionDepositDao;

        public BidRepository(GenericDao<Auction> auctionDao, GenericDao<Bid> bidDao,
            GenericDao<AuctionDeposit> auctionDepositDao)
        {
            _auctionDao = auctionDao;
            _bidDao = bidDao;
            _auctionDepositDao = auctionDepositDao;
        }

        public async Task<BidDetailResponse?> CreateBid(Guid id, CreateBidRequest request)
        {
            var auction = await _auctionDao.GetQueryable().Include(x => x.AuctionFashionItem)
                .FirstOrDefaultAsync(x => x.AuctionId == id);

            if (auction == null)
            {
                throw new AuctionNotFoundException();
            }

            if (auction.EndDate <= DateTime.UtcNow || auction.Status == AuctionStatus.Finished)
            {
                throw new InvalidOperationException("Auction not found or have ended");
            }

            if (auction.Status != AuctionStatus.OnGoing)
            {
                throw new InvalidOperationException("Auction has not started yet");
            }

            var auctionDeposit = await _auctionDepositDao.GetQueryable()
                .FirstOrDefaultAsync(x => x.AuctionId == id && x.MemberId == request.MemberId);

            if (auctionDeposit == null)
            {
                throw new AuctionDepositNotFoundException();
            }

            var latestBid = await _bidDao.GetQueryable()
                .OrderByDescending(x => x.CreatedDate)
                .FirstOrDefaultAsync(x => x.AuctionId == id);

            if (latestBid != null && request.MemberId == latestBid.MemberId)
            {
                throw new InvalidOperationException("You have already bid on this auction");
            }

            var currentBidRequired = latestBid != null
                ? latestBid.Amount + auction.StepIncrement
                : auction.AuctionFashionItem.InitialPrice;

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
                CreatedDate = DateTime.UtcNow
            };

            var result = await _bidDao.AddAsync(newBid);

            if (latestBid != null)
            {
                latestBid.IsWinning = false;
                await _bidDao.UpdateAsync(latestBid);
            }

            return new BidDetailResponse
            {
                AuctionId = id,
                Amount = result.Amount,
                MemberId = result.MemberId,
                Id = result.BidId,
                IsWinning = true,
                CreatedDate = result.CreatedDate,
                NextAmount = result.Amount + auction.StepIncrement
            };
        }

        public async Task<PaginationResponse<BidListResponse>?> GetBids(Guid id, GetBidsRequest request)
        {
            var items = await _bidDao.GetQueryable()
                .Where(x => x.AuctionId == id)
                .Take(request.PageSize)
                .Skip((request.PageNumber - 1) * request.PageSize)
                .OrderByDescending(x => x.CreatedDate)
                .Select(x => new BidListResponse
                {
                    Amount = x.Amount,
                    AuctionId = x.AuctionId,
                    Id = x.BidId,
                    MemberId = x.MemberId,
                    CreatedDate = x.CreatedDate,
                    IsWinning = x.IsWinning
                })
                .ToListAsync();

            var count = await _bidDao.GetQueryable().Where(x => x.AuctionId == id).CountAsync();

            return new PaginationResponse<BidListResponse>
            {
                Items = items,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize,
                TotalCount = count,
                Filters = ["AuctionId"],
                OrderBy = "-CreatedDate"
            };
        }

        public async Task<BidDetailResponse?> GetLargestBid(Guid auctionId)
        {
            var result = await _bidDao.GetQueryable()
                    .OrderByDescending(x => x.Amount)
                    .Select(x => new BidDetailResponse()
                    {
                        Amount = x.Amount,
                        AuctionId = x.AuctionId,
                        Id = x.BidId,
                        MemberId = x.MemberId,
                        CreatedDate = x.CreatedDate,
                        IsWinning = x.IsWinning
                    })
                    .FirstOrDefaultAsync(x => x.AuctionId == auctionId)
                ;

            return result;
        }
    }
}