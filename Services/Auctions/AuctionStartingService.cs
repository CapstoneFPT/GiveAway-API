using BusinessObjects.Dtos.Commons;
using Dao;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Repositories.Auctions;

namespace Services.Auctions;

public class AuctionStartingService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<AuctionStartingService> _logger;
    private const int CheckInterval = 1000 * 60 * 5;

    public AuctionStartingService(IServiceProvider serviceProvider, ILogger<AuctionStartingService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await using var scope = _serviceProvider.CreateAsyncScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<GiveAwayDbContext>();
            var auctionToStart = await dbContext.Auctions
                .Where(a => a.StartDate <= DateTime.UtcNow && a.Status == AuctionStatus.Approved)
                .Include(auction => auction.IndividualAuctionFashionItem)
                .ToListAsync(stoppingToken);

            foreach (var auction in auctionToStart)
            {
                try
                {
                    auction.Status = AuctionStatus.OnGoing;
                    auction.IndividualAuctionFashionItem.Status = FashionItemStatus.Bidding;


                    _logger.LogInformation("Auction {AuctionId} has been started", auction);
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Failed to start auction {AuctionId}", auction);
                }
            }

            dbContext.Auctions.UpdateRange(auctionToStart);

            await dbContext.SaveChangesAsync(stoppingToken);

            await Task.Delay(CheckInterval, stoppingToken);
        }
    }
}