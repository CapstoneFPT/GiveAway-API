using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessObjects.Dtos.AuctionDeposits;
using BusinessObjects.Dtos.Auctions;
using BusinessObjects.Dtos.Bids;
using BusinessObjects.Dtos.Commons;

namespace Services.Auctions
{
    public interface IAuctionService
    {
        Task<AuctionDetailResponse> CreateAuction(CreateAuctionRequest request);
        Task<PaginationResponse<AuctionListResponse>> GetAuctions(GetAuctionsRequest request);
        Task<AuctionDetailResponse?> GetAuction(Guid id);
        Task<AuctionDetailResponse?> DeleteAuction(Guid id);
        Task<AuctionDetailResponse> UpdateAuction(Guid id, UpdateAuctionRequest request);
        Task<AuctionDepositDetailResponse> PlaceDeposit(Guid auctionId, CreateAuctionDepositRequest request);
        Task<AuctionDepositDetailResponse?> GetDeposit(Guid id, Guid depositId);
        Task<AuctionDetailResponse?> ApproveAuction(Guid id);
        Task<AuctionDetailResponse?> RejectAuction(Guid id);
        Task<BidDetailResponse?> PlaceBid(Guid id, CreateBidRequest request);
        Task<PaginationResponse<BidListResponse>?> GetBids(Guid id, GetBidsRequest request);
        Task<BidDetailResponse?> GetLargestBid(Guid auctionId);
    }
}
