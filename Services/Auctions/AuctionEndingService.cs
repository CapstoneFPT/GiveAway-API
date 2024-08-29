using BusinessObjects.Dtos.Auctions;
using BusinessObjects.Dtos.Commons;
using BusinessObjects.Entities;
using Dao;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Repositories.Auctions;
using Repositories.Orders;

namespace Services.Auctions;

public class AuctionEndingService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<AuctionEndingService> _logger;
    private const int CheckInterval = 1000 * 60 * 10;

    public AuctionEndingService(IServiceProvider serviceProvider, ILogger<AuctionEndingService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }


    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CheckAndEndAuction();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Auction ending service error");
            }

            await Task.Delay(CheckInterval, stoppingToken);
        }
    }

    private async Task CheckAndEndAuction()
    {
        await using var scope = _serviceProvider.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<GiveAwayDbContext>();

        var auctionToEnd = await dbContext.Auctions
            .Where(a => a.StartDate <= DateTime.UtcNow && a.Status == AuctionStatus.Approved).ToListAsync();

        foreach (var auction in auctionToEnd)
        {
            try
            {
                var winningBid = await dbContext.Bids.Where(x => x.AuctionId == auction.AuctionId)
                    .OrderByDescending(x => x.Amount).FirstOrDefaultAsync();
                auction.Status = AuctionStatus.Finished;
                dbContext.Auctions.Update(auction);

                var orderRepository = scope.ServiceProvider.GetRequiredService<IOrderRepository>();

                var orderRequest = new CreateOrderFromBidRequest()
                {
                    MemberId = winningBid.MemberId,
                    OrderCode = orderRepository.GenerateUniqueString(),
                    PaymentMethod = PaymentMethod.Point,
                    TotalPrice = winningBid.Amount,
                    BidId = winningBid.BidId,
                    AuctionFashionItemId = auction.IndividualAuctionFashionItemId
                };

                var newOrder = new Order()
                {
                    BidId = orderRequest.BidId,
                    OrderCode = orderRequest.OrderCode,
                    PaymentMethod = orderRequest.PaymentMethod,
                    MemberId = orderRequest.MemberId,
                    TotalPrice = orderRequest.TotalPrice,
                    CreatedDate = DateTime.UtcNow,
                };
                dbContext.Orders.Add(newOrder);


                var orderDetail = new OrderLineItem()
                {
                    OrderId = newOrder.OrderId,
                    IndividualFashionItemId = orderRequest.AuctionFashionItemId,
                    UnitPrice = orderRequest.TotalPrice,
                };

                dbContext.OrderLineItems.Add(orderDetail);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to end auction {AuctionId}", auction);
            }
        }

        await dbContext.SaveChangesAsync();
    }
}