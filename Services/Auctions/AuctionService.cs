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

        public Task<List<AuctionListResponse>> GetAuctions()
        {
            throw new NotImplementedException();
        }
    }
}