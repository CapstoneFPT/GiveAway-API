using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessObjects.Dtos.Auctions;
using BusinessObjects.Dtos.Commons;
using Repositories.Auctions;

namespace Services.Auctions
{
    public class AuctionService : IAuctionService
    {
        private IAuctionRepository _auctionRepository;

        public AuctionService(IAuctionRepository auctionRepository)
        {
            _auctionRepository = auctionRepository;
        }

        public async Task<AuctionDetailResponse> CreateAuction(CreateAuctionRequest request)
        {
            try
            {
                var result = await _auctionRepository.CreateAuction(request);
                return result;
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        public Task<PaginationResponse<AuctionListResponse>> GetAuctions(GetAuctionsRequest request)
        {
            try
            {
                var result = _auctionRepository.GetAuctions(request);
                return result;
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        public Task<AuctionDetailResponse?> GetAuction(Guid id)
        {
            try
            {
                var result = _auctionRepository.GetAuction(id);
                return result;
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        public Task<AuctionDetailResponse?> DeleteAuction(Guid id)
        {
            try
            {
                var result = _auctionRepository.DeleteAuction(id);
                return result;
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        public Task<AuctionDetailResponse> UpdateAuction(Guid id, UpdateAuctionRequest request)
        {
            try
            {
var result = _auctionRepository.UpdateAuction(id, request);
return result;
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }
    }
}