using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessObjects.Dtos.AuctionDeposits;
using BusinessObjects.Dtos.Commons;

namespace Repositories.AuctionDeposits
{
    public interface IAuctionDepositRepository
    {
        Task<AuctionDepositDetailResponse> CreateDeposit(Guid auctionId, CreateAuctionDepositRequest request);
        Task<PaginationResponse<AuctionDepositListResponse>> GetAuctionDeposits(Guid auctionId, GetDepositsRequest request);
    }
}
