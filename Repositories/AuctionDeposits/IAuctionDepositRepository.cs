using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using BusinessObjects.Dtos.AuctionDeposits;
using BusinessObjects.Dtos.Commons;
using BusinessObjects.Entities;

namespace Repositories.AuctionDeposits
{
    public interface IAuctionDepositRepository
    {
        Task<AuctionDepositDetailResponse> CreateDeposit(Guid auctionId, CreateAuctionDepositRequest request, Guid transactionId);
        Task<PaginationResponse<AuctionDepositListResponse>> GetAuctionDeposits(Guid auctionId, GetDepositsRequest request);
        Task<T?> GetSingleDeposit<T>(Expression<Func<AuctionDeposit, bool>>? predicate,
            Expression<Func<AuctionDeposit, T>>? selector);

        Task<AuctionDeposit> CreateAuctionDeposit(AuctionDeposit deposit);
        IQueryable<AuctionDeposit> GetQueryable();
    }
}
