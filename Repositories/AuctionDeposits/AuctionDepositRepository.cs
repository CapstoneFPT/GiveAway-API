using System.Linq.Expressions;
using BusinessObjects.Dtos.AuctionDeposits;
using BusinessObjects.Dtos.Commons;
using BusinessObjects.Entities;
using BusinessObjects.Utils;
using Dao;
using Microsoft.EntityFrameworkCore;
using Transaction = BusinessObjects.Entities.Transaction;

namespace Repositories.AuctionDeposits
{
    public class AuctionDepositRepository : IAuctionDepositRepository
    {
        private readonly GenericDao<AuctionDeposit> _auctionDepositDao;
        private readonly GenericDao<Transaction> _transactionDao;
        private readonly GenericDao<Auction> _auctionDao;

        public AuctionDepositRepository(GenericDao<AuctionDeposit> auctionDepositDao,
            GenericDao<Transaction> transactionDao, GenericDao<Auction> auctionDao)
        {
            _auctionDepositDao = auctionDepositDao;
            _transactionDao = transactionDao;
            _auctionDao = auctionDao;
        }

        public async Task<AuctionDepositDetailResponse> CreateDeposit(Guid auctionId,
            CreateAuctionDepositRequest request)
        {
            var existingDeposit = await _auctionDepositDao.GetQueryable()
                .FirstOrDefaultAsync(x => x.AuctionId == auctionId && x.MemberId == request.MemberId);

            var auction = await _auctionDao.GetQueryable()
                .FirstOrDefaultAsync(x => x.AuctionId == auctionId);

            if (auction == null)
            {
                throw new AuctionNotFoundException();
            }

            var timeDiff = auction.StartDate - DateTime.UtcNow;
            if (timeDiff.TotalHours < 24)
            {
                throw new InvalidOperationException("Deposit can only be made after 24 hours of auction start");
            }

            if (existingDeposit != null)
            {
                throw new InvalidOperationException("Member has already placed a deposit");
            }

            var transaction = new Transaction
            {
                Amount = auction.DepositFee,
                Type = TransactionType.AuctionDeposit,
                MemberId = request.MemberId,
                CreatedDate = DateTime.UtcNow
            };

            var transactionResult = await _transactionDao.AddAsync(transaction);

            var deposit = new AuctionDeposit()
            {
                MemberId = request.MemberId,
                AuctionId = auctionId,
                TransactionId = transactionResult.TransactionId,
                CreatedDate = DateTime.UtcNow
            };

            var result = await _auctionDepositDao.AddAsync(deposit);
            return new AuctionDepositDetailResponse
            {
                Id = result.AuctionDepositId,
                TransactionId = transactionResult.TransactionId,
                AuctionId = auctionId,
                Amount = transactionResult.Amount,
                MemberId = request.MemberId,
            };
        }

        public async Task<PaginationResponse<AuctionDepositListResponse>> GetAuctionDeposits(Guid auctionId,
            GetDepositsRequest request)
        {
            var data = await _auctionDepositDao.GetQueryable()
                .Where(x => x.AuctionId == auctionId)
                .Select(x => new AuctionDepositListResponse()
                {
                    MemberId = x.MemberId,
                    AuctionId = x.AuctionId,
                    DepositDate = x.CreatedDate,
                    Id = x.AuctionDepositId
                })
                .ToListAsync();

            var result = new PaginationResponse<AuctionDepositListResponse>()
            {
                Items = data,
            };

            return result;
        }

        public Task<T?> GetSingleDeposit<T>(Expression<Func<AuctionDeposit, bool>>? predicate,
            Expression<Func<AuctionDeposit, T>>? selector)
        {
            var query = _auctionDepositDao.GetQueryable();
            if (predicate != null)
            {
                query = query.Where(predicate);
            }
            
            if(selector != null)
            {
                return query
                    .Select(selector)
                    .FirstOrDefaultAsync();
            }

            return query.Cast<T>().SingleOrDefaultAsync();
        }
    }
}