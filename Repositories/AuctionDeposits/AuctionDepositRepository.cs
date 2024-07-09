using BusinessObjects.Dtos.AuctionDeposits;
using BusinessObjects.Dtos.Commons;
using BusinessObjects.Entities;
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
            GenericDao<Transaction> transactionDao,  GenericDao<Auction> auctionDao)
        {
            _auctionDepositDao = auctionDepositDao;
            _transactionDao = transactionDao;
            _auctionDao = auctionDao;
        }

        public async Task<AuctionDepositDetailResponse> CreateDeposit(Guid auctionId,
            CreateAuctionDepositRequest request)
        {
            try
            {
                var existingDeposit = await _auctionDepositDao.GetQueryable()
                    .FirstOrDefaultAsync(x => x.AuctionId == auctionId && x.MemberId == request.MemberId);

                var auction = await _auctionDao.GetQueryable()
                    .FirstOrDefaultAsync(x => x.AuctionId == auctionId);

                if (auction == null)
                {
                    throw new Exception("Auction Not found");
                }

                var timeDiff = auction.StartDate - DateTime.UtcNow;
                if (timeDiff.TotalHours < 24)
                {
                    throw new Exception("Deposit period is over");
                }

                if (existingDeposit != null)
                {
                    throw new Exception("Deposit already exists");
                }

                var transaction = new Transaction
                {
                    Amount = auction.DepositFee,
                    Type = TransactionType.AuctionDeposit,
                    MemberId = request.MemberId,
                    CreatedDate = DateTime.UtcNow
                };
                
                var transactionResult =  await _transactionDao.AddAsync(transaction);
                
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
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }
    }
}