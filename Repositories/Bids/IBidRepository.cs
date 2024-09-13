using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessObjects.Dtos.Bids;
using BusinessObjects.Dtos.Commons;
using BusinessObjects.Entities;

namespace Repositories.Bids
{
    public interface IBidRepository
    {
        Task<BidDetailResponse?> CreateBid(Guid id, CreateBidRequest request);
        Task<BidDetailResponse?> GetLargestBid(Guid auctionId);
        IQueryable<Bid> GetQueryable();
    }
}
