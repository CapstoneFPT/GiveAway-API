using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessObjects.Dtos.Bids;
using BusinessObjects.Dtos.Commons;
using BusinessObjects.Entities;
using Dao;
using Microsoft.EntityFrameworkCore;

namespace Repositories.Bids
{
    public class BidRepository : IBidRepository
    {
        private readonly GenericDao<Auction> _auctionDao;
        private readonly GenericDao<Bid> _bidDao;
        private readonly GenericDao<Account> _memberDao;
        private readonly GenericDao<AuctionDeposit> _auctionDepositDao;

        public BidRepository(GenericDao<Auction> auctionDao, GenericDao<Bid> bidDao,GenericDao<Account> memberDao,
             GenericDao<AuctionDeposit> auctionDepositDao)
        {
            _auctionDao = auctionDao;
            _bidDao = bidDao;
            _memberDao = memberDao;
            _auctionDepositDao = auctionDepositDao;
        }

        public async Task<BidDetailResponse?> CreateBid(Guid id, CreateBidRequest request)
        {
            try
            {
                var auction = await _auctionDao.GetQueryable().Include(x => x.AuctionFashionItem)
                    .FirstOrDefaultAsync(x => x.AuctionId == id);

                if (auction == null || auction.EndDate <= DateTime.UtcNow || auction.Status == AuctionStatus.Finished)
                {
                    throw new Exception("Auction not found or have ended");
                }

                if (auction.Status != AuctionStatus.OnGoing)
                {
                    throw new InvalidOperationException("Auction has not started yet");
                }

                var auctionDeposit = await _auctionDepositDao.GetQueryable()
                    .FirstOrDefaultAsync(x => x.AuctionId == id && x.MemberId == request.MemberId);
                
                if (auctionDeposit == null)
                {
                    throw new Exception("Member has not place a deposit yet");
                }

                var latestBid = await _bidDao.GetQueryable()
                    .OrderByDescending(x => x.CreatedDate)
                    .FirstOrDefaultAsync(x => x.AuctionId == id);

                if (latestBid != null && request.MemberId == latestBid.MemberId)
                {
                    throw new InvalidOperationException("You have already bid on this auction");
                }

                var nextBidAmount = latestBid != null ? latestBid.Amount + auction.StepIncrement : auction.AuctionFashionItem.InitialPrice;

                if (request.Amount != nextBidAmount)
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
                    NextAmount = result.Amount + auction.StepIncrement 
                };
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        public async Task<PaginationResponse<BidListResponse>?> GetBids(Guid id, GetBidsRequest request)
        {
            try
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
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }
    }
}