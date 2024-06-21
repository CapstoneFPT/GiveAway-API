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
        private readonly GenericDao<Account> _memberDao;
        private readonly GenericDao<Wallet> _walletDao;
        private readonly GenericDao<Auction> _auctionDao;

        public AuctionDepositRepository(GenericDao<AuctionDeposit> auctionDepositDao,
            GenericDao<Transaction> transactionDao, GenericDao<Account> memberDao, GenericDao<Wallet> walletDao, GenericDao<Auction> auctionDao)
        {
            _auctionDepositDao = auctionDepositDao;
            _transactionDao = transactionDao;
            _memberDao = memberDao;
            _walletDao = walletDao;
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

                var timeDiff = DateTime.UtcNow - auction.StartDate;
                if (timeDiff.Hours < 24)
                {
                    throw new Exception("Deposit period is over");
                }

                if (existingDeposit != null)
                {
                    throw new Exception("Deposit already exists");
                }

                var wallet = await _walletDao.GetQueryable()
                    .FirstOrDefaultAsync(x => x.MemberId == request.MemberId);

                if (wallet == null)
                {
                    throw new Exception("Wallet not found");
                }

                var transaction = new Transaction
                {
                    Amount = auction.DepositFee,
                    Type = TransactionType.AuctionDeposit,
                    WalletId = wallet.WalletId,
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