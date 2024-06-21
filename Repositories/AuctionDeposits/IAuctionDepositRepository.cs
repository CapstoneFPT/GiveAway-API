using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessObjects.Dtos.AuctionDeposits;

namespace Repositories.AuctionDeposits
{
    public interface IAuctionDepositRepository
    {
        Task<AuctionDepositDetailResponse> CreateDeposit(Guid auctionId, CreateAuctionDepositRequest request);
    }
}
