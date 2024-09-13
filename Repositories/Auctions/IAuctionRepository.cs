using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using BusinessObjects.Dtos.Auctions;
using BusinessObjects.Dtos.Bids;
using BusinessObjects.Dtos.Commons;
using BusinessObjects.Entities;

namespace Repositories.Auctions
{
    public interface IAuctionRepository
    {
        Task<AuctionDetailResponse> CreateAuction(Auction request);
        Task<Auction?> GetAuction(Guid id, bool includeRelations = false);
        Task<AuctionDetailResponse?> DeleteAuction(Guid id);
        Task<AuctionDetailResponse> UpdateAuction(Guid id, UpdateAuctionRequest request);
        Task<AuctionDetailResponse?> ApproveAuction(Guid id);
        Task<RejectAuctionResponse?> RejectAuction(Guid id);
        Task<Auction> UpdateAuctionStatus(Guid auctionId, AuctionStatus auctionStatus);
        Task<List<Guid>> GetAuctionEndingNow();
        Task<List<Guid>> GetAuctionStartingNow();
        Task<(List<T> Items, int Page, int PageSize, int Total)> GetAuctionProjections<T>(int? requestPageNumber,
            int? requestPageSize, Expression<Func<Auction, bool>> predicate, Expression<Func<Auction, T>> selector);

         IQueryable<Auction> GetQueryable();
    }
}
