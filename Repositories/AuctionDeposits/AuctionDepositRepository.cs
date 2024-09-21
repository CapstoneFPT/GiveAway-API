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
        private const string Prefix = "DEP";
        private static Random _random = new();
        public async Task<string> GenerateUniqueString()
        {
            for (int attempt = 0; attempt < 5; attempt++)
            {
                string code = GenerateCode();
                bool isCodeExisted = await GenericDao<AuctionDeposit>.Instance.GetQueryable().AnyAsync(r => r.DepositCode == code);

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

        public async Task<AuctionDeposit> CreateAuctionDeposit(AuctionDeposit deposit)
        {
            deposit.DepositCode = await GenerateUniqueString();
            return await GenericDao<AuctionDeposit>.Instance.AddAsync(deposit);
        }

        public IQueryable<AuctionDeposit> GetQueryable()
        {
            return GenericDao<AuctionDeposit>.Instance.GetQueryable();
        }

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
                DepositCode = await GenerateUniqueString(),
                CreatedDate = DateTime.UtcNow
            };

            var result = await GenericDao<AuctionDeposit>.Instance.AddAsync(deposit);
            return new AuctionDepositDetailResponse
            {
                Id = result.AuctionDepositId,
                TransactionId = transactionId,
                AuctionId = auctionId,
                DepositCode = deposit.DepositCode,
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
                    CustomerName = x.Member.Fullname,
                    CustomerEmail = x.Member.Email,
                    CustomerPhone = x.Member.Phone,
                    Amount = x.Auction.DepositFee,
                    AuctionId = x.AuctionId,
                    DepositDate = x.CreatedDate,
                    Id = x.AuctionDepositId,
                    DepositCode = x.DepositCode
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

            if (selector != null)
            {
                return query
                    .Select(selector)
                    .FirstOrDefaultAsync();
            }

            return query.Cast<T>().SingleOrDefaultAsync();
        }
    }
}