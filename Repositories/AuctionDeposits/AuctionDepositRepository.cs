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
     

    
        public async Task<AuctionDepositDetailResponse> CreateDeposit(Guid auctionId,
            CreateAuctionDepositRequest request, Guid transactionId)
        {
            var existingDeposit = await GenericDao<AuctionDeposit>.Instance.GetQueryable()
                .FirstOrDefaultAsync(x => x.AuctionId == auctionId && x.MemberId == request.MemberId);

            var auction = await GenericDao<Auction>.Instance.GetQueryable()
                .FirstOrDefaultAsync(x => x.AuctionId == auctionId);

            if (auction == null)
            {
                throw new AuctionNotFoundException();
            }

            var timeDiff = auction.StartDate - DateTime.UtcNow;
            // if (timeDiff.TotalHours < 24)
            // {
            //     throw new InvalidOperationException("Deposit can only be made before 24 hours of auction start");
            // }

            if (existingDeposit != null)
            {
                throw new InvalidOperationException("Member has already placed a deposit");
            }


            var deposit = new AuctionDeposit()
            {
                MemberId = request.MemberId,
                AuctionId = auctionId,
                TransactionId = transactionId,
                CreatedDate = DateTime.UtcNow
            };

            var result = await GenericDao<AuctionDeposit>.Instance.AddAsync(deposit);
            return new AuctionDepositDetailResponse
            {
                Id = result.AuctionDepositId,
                TransactionId = transactionId,
                AuctionId = auctionId,
                Amount = auction.DepositFee,
                MemberId = request.MemberId,
            };
        }

        public async Task<PaginationResponse<AuctionDepositListResponse>> GetAuctionDeposits(Guid auctionId,
            GetDepositsRequest request)
        {
            var data = await GenericDao<AuctionDeposit>.Instance.GetQueryable()
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
            var query = GenericDao<AuctionDeposit>.Instance.GetQueryable();
            if (predicate != null)
            {
                query = query.Where(predicate);
            }

            query = query.Include(x => x.Auction);
            
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