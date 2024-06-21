using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessObjects.Dtos.Bids;
using BusinessObjects.Dtos.Commons;

namespace Repositories.Bids
{
    public interface IBidRepository
    {
        Task<BidDetailResponse?> CreateBid(Guid id, CreateBidRequest request);
        Task<PaginationResponse<BidListResponse>?> GetBids(Guid id, GetBidsRequest request);
    }
}
