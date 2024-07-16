using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Repositories.Auctions;

namespace Services.Auctions;

public class AuctionEndingService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<AuctionEndingService> _logger;
    private const int CheckInterval = 10000;
    
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
        using var scope = _serviceProvider.CreateScope();
        var auctionService = scope.ServiceProvider.GetRequiredService<IAuctionService>();
        var auctionRepository = scope.ServiceProvider.GetRequiredService<IAuctionRepository>();
        
        var auctionToEnd = await auctionRepository.GetAuctionEndingNow();

        foreach (var auction in auctionToEnd)
        {
            try
            {

                await auctionService.EndAuction(auction);
                _logger.LogInformation("Auction {AuctionId} has been ended", auction);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to end auction {AuctionId}", auction);
            } 
        }
    }
}