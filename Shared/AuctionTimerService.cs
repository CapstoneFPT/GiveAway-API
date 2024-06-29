using BusinessObjects.Dtos.Commons;
using Dao;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Shared;

public class AuctionTimerService : BackgroundService
{

    private readonly IServiceScopeFactory _serviceScopeFactory;
    public AuctionTimerService(IServiceScopeFactory serviceScopeFactory)
    {
        _serviceScopeFactory = serviceScopeFactory;
    }
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using (var scope = _serviceScopeFactory.CreateScope())
            {
                var giveAwayContext = scope.ServiceProvider.GetRequiredService<GiveAwayDbContext>();
                var now = DateTime.Now;
                var auctions = await giveAwayContext.Auctions
                    .Where(x => x.StartDate <= now && x.EndDate >= now && x.Status == AuctionStatus.Finished)
                    .ToListAsync(cancellationToken: stoppingToken);
                foreach (var auction in auctions)
                {
                    var highestBid = await giveAwayContext.Bids
                        .Where(x => x.AuctionId == auction.AuctionId)
                        .OrderByDescending(x => x.Amount)
                        .FirstOrDefaultAsync(cancellationToken: stoppingToken);

                    var winner = highestBid?.MemberId ?? null;
                    var auctionEndedEvent = new AuctionEndedEvent()
                    {
                        AuctionId = auction.AuctionId,
                    };

                    await PublishEvent(auctionEndedEvent);

                    auction.Status = AuctionStatus.Finished;
                    giveAwayContext.Auctions.Update(auction);
                    await giveAwayContext.SaveChangesAsync(stoppingToken);
                }
            }
            
            await Task.Delay(2000, stoppingToken);
        } 
    }

    private async Task PublishEvent<TEvent>(TEvent auctionEvent)
    {
        using (var scope = _serviceScopeFactory.CreateScope())
        {
            var handlers = scope.ServiceProvider.GetServices<IEventHandler<TEvent>>();
            foreach (var handler in handlers)
            {
                await handler.Handle(auctionEvent);
            }
        }
    }
}
