﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessObjects.Dtos.Auctions;
using BusinessObjects.Dtos.Bids;
using BusinessObjects.Dtos.Commons;

namespace Repositories.Auctions
{
    public interface IAuctionRepository
    {
        Task<AuctionDetailResponse> CreateAuction(CreateAuctionRequest request);
        Task<PaginationResponse<AuctionListResponse>> GetAuctions(GetAuctionsRequest request);
        Task<AuctionDetailResponse?> GetAuction(Guid id);
        Task<AuctionDetailResponse?> DeleteAuction(Guid id);
        Task<AuctionDetailResponse> UpdateAuction(Guid id, UpdateAuctionRequest request);
        Task<AuctionDetailResponse?> ApproveAuction(Guid id);
        Task<AuctionDetailResponse?> RejectAuction(Guid id);
    }
}
