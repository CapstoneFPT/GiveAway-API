﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessObjects.Dtos.AuctionDeposits;
using BusinessObjects.Dtos.Auctions;
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
        Task<AuctionDepositDetailResponse> CreateDeposit(Guid id, CreateAuctionDepositRequest request);
        Task<AuctionDepositDetailResponse?> GetDeposit(Guid id, Guid depositId);
        Task<AuctionDetailResponse?> ApproveAuction(Guid id);
        Task<AuctionDetailResponse?> RejectAuction(Guid id);
    }
}
