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
            using var scope = _serviceProvider.CreateScope();
            var auctionService = scope.ServiceProvider.GetRequiredService<IAuctionService>();
            var auctionRepository = scope.ServiceProvider.GetRequiredService<IAuctionRepository>();
            var auctionToStart = await auctionRepository.GetAuctionStartingNow();
            foreach (var auction in auctionToStart)
            {
                try
                {
                    await auctionService.StartAuction(auction);
                    _logger.LogInformation("Auction {AuctionId} has been started", auction);
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Failed to start auction {AuctionId}", auction);
                }
            }
            await Task.Delay(CheckInterval, stoppingToken);
        }
    }
}