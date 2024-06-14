using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessObjects.Dtos.Auctions;
using BusinessObjects.Dtos.Commons;

namespace Services.Auctions
{
    public interface IAuctionService
    {
        Task<AuctionDetailResponse> CreateAuction(CreateAuctionRequest request);
        Task<PaginationResponse<AuctionListResponse>> GetAuctions(GetAuctionsRequest request);
        Task<AuctionDetailResponse?> GetAuction(Guid id);
    }
}
