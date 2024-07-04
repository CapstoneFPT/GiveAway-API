using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessObjects.Dtos.AuctionDeposits;
using BusinessObjects.Dtos.Auctions;
using BusinessObjects.Dtos.Bids;
using BusinessObjects.Dtos.Commons;
using BusinessObjects.Dtos.Orders;
using BusinessObjects.Entities;
using Microsoft.Extensions.DependencyInjection;
using Repositories.AuctionDeposits;
using Repositories.AuctionItems;
using Repositories.Auctions;
using Repositories.Bids;
using Repositories.OrderDetails;
using Repositories.Orders;
using Services.Orders;

namespace Services.Auctions
{
    public class AuctionService : IAuctionService
    {
        private readonly IAuctionRepository _auctionRepository;
        private readonly IBidRepository _bidRepository;
        private readonly IAuctionDepositRepository _auctionDepositRepository;
        private readonly IServiceProvider _serviceProvider;
        private readonly IOrderRepository _orderRepository;
        private readonly IAuctionItemRepository _auctionItemRepository;

        public AuctionService(IAuctionRepository auctionRepository, IBidRepository bidRepository,
            IAuctionDepositRepository auctionDepositRepository, IServiceProvider serviceProvider,IOrderRepository orderRepository,IAuctionItemRepository auctionItemRepository)
        {
            _auctionRepository = auctionRepository;
            _bidRepository = bidRepository;
            _auctionDepositRepository = auctionDepositRepository;
            _serviceProvider = serviceProvider;
            _orderRepository = orderRepository;
            _auctionItemRepository = auctionItemRepository;
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

        public async Task<Result<OrderResponse>> EndAuction(Guid id)
        {
            var auction = await _auctionRepository.GetAuction(id);
            if (auction is null)
            {
                return new Result<OrderResponse>()
                {
                    ResultStatus = ResultStatus.NotFound,
                    Messages = new[] { "Auction Not Found" }
                };
            }

            if (auction.Status != AuctionStatus.OnGoing)
            {
                return new Result<OrderResponse>()
                {
                    ResultStatus = ResultStatus.Error,
                    Messages = new[] { "Auction is not on going" }
                };
            }

            var winningBid = await _bidRepository.GetLargestBid(id);
            if (winningBid is null)
            {
                await _auctionRepository.UpdateAuctionStatus(auctionId: id, auctionStatus: AuctionStatus.Finished);
                return new Result<OrderResponse>()
                {
                    ResultStatus = ResultStatus.Empty, Messages = new[] { "No Bids" }
                };
            }

            using var scope = _serviceProvider.CreateScope();
            var orderService = scope.ServiceProvider.GetRequiredService<IOrderService>();

            var createOrderRequest = new CreateOrderFromBidRequest()
            {
                MemberId = winningBid.MemberId,
                OrderCode = OrderRepository.GenerateUniqueString(),
                PaymentMethod = PaymentMethod.Point,
                TotalPrice = winningBid.Amount,
                BidId = winningBid.Id,
                AuctionFashionItemId = auction.AuctionItemId
            };


            var orderResult = await orderService.CreateOrderFromBid(createOrderRequest);

            if (orderResult.ResultStatus != ResultStatus.Success)
            {
                return new Result<OrderResponse>()
                {
                    ResultStatus = ResultStatus.Error,
                    Messages = new[] { "Failed to create order" }
                };
            }

            await _auctionRepository.UpdateAuctionStatus(auctionId: id, auctionStatus: AuctionStatus.Finished);

            return new Result<OrderResponse>()
            {
                ResultStatus = ResultStatus.Success,
                Messages = new[] { "Auction Ended Successfully and Order Created" }, Data = orderResult.Data
            };
        }

        public async Task StartAuction(Guid auctionId)
        {
            try
            {
                var auctionUpdateResult = await _auctionRepository.UpdateAuctionStatus(auctionId, AuctionStatus.OnGoing);

                var auctionFashionItemId = auctionUpdateResult
                    .AuctionFashionItemId;
                
                var auctionItemUpdateResult = await _auctionItemRepository
                    .UpdateAuctionItemStatus(auctionFashionItemId, FashionItemStatus.Bidding);

            }
            catch (Exception e)
            {
                throw new Exception(e.Message)
                    ;
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


                return result;
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
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

        public Task<BidDetailResponse?> GetLargestBid(Guid auctionId)
        {
            try
            {
                var result = _bidRepository.GetLargestBid(auctionId);
                return result;
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

       
    }
}