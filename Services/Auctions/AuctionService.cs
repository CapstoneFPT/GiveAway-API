using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessObjects.Dtos.AuctionDeposits;
using BusinessObjects.Dtos.Auctions;
using BusinessObjects.Dtos.Bids;
using BusinessObjects.Dtos.Commons;
using BusinessObjects.Entities;
using Microsoft.Extensions.DependencyInjection;
using Repositories.AuctionDeposits;
using Repositories.Auctions;
using Repositories.Bids;
using Shared;

namespace Services.Auctions
{
    public class AuctionService : IAuctionService
    {
        private readonly IAuctionRepository _auctionRepository;
        private readonly IBidRepository _bidRepository;
        private readonly IAuctionDepositRepository _auctionDepositRepository;
        private readonly IServiceProvider _serviceProvider;

        public AuctionService(IAuctionRepository auctionRepository, IBidRepository bidRepository, IAuctionDepositRepository auctionDepositRepository, IServiceProvider serviceProvider)
        {
            _auctionRepository = auctionRepository;
            _bidRepository = bidRepository;
            _auctionDepositRepository = auctionDepositRepository;
            _serviceProvider = serviceProvider;
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

        public Task<AuctionDepositDetailResponse> PlaceDeposit(Guid auctionId, CreateAuctionDepositRequest request)
        {
            try
            {
               var result = _auctionDepositRepository.CreateDeposit(auctionId, request);
               return result;
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        public Task<AuctionDepositDetailResponse?> GetDeposit(Guid id, Guid depositId)
        {
            throw new NotImplementedException();
        }

        public Task<AuctionDetailResponse?> ApproveAuction(Guid id)
        {
            try
            {
                var result = _auctionRepository.ApproveAuction(id);
                return result;
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        public Task<AuctionDetailResponse?> RejectAuction(Guid id)
        {
            try
            {
                var result = _auctionRepository.RejectAuction(id);
                return result;
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        public async Task<BidDetailResponse?> PlaceBid(Guid id, CreateBidRequest request)
        {
            try
            {
                var result = await _bidRepository.CreateBid(id, request);
                if (result == null)
                {
                    throw new Exception("Bid not created");
                }
                var bidPlacedEvent = new BidPlacedEvent()
                {
                    AuctionId = id,
                    Bid = result
                };
                await PublishEvent(bidPlacedEvent);
                return result;
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        private async Task PublishEvent<TEvent>(TEvent auctionEvent)
        {
            var handlers = _serviceProvider.GetServices<IEventHandler<TEvent>>();
            foreach (var handler in handlers)
            {
                await handler.Handle(auctionEvent);
            }
        }

        public Task<PaginationResponse<BidListResponse>?> GetBids(Guid id, GetBidsRequest request)
        {
            try
            {
                var result = _bidRepository.GetBids(id, request);
                return result;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
    }
}