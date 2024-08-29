using BusinessObjects.Dtos.Commons;
using BusinessObjects.Entities;
using BusinessObjects.Utils;
using Dao;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Repositories.FashionItems;

namespace Services.FashionItems;

public class CompleteRefundableItemsService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private const int TimeInterval = 1000 * 60 * 60 * 24;
    private readonly ILogger<CompleteRefundableItemsService> _logger;

    public CompleteRefundableItemsService(IServiceProvider serviceProvider,
        ILogger<CompleteRefundableItemsService> logger)
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
                await CheckAndChangeToSoldRefundableItems();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to execute CompleteRefundableItemsService");
            }

            await Task.Delay(TimeInterval, stoppingToken);
        }
    }

    private async Task CheckAndChangeToSoldRefundableItems()
    {
        try
        {
            await using var scope = _serviceProvider.CreateAsyncScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<GiveAwayDbContext>();


            var refundableItems = await dbContext.OrderLineItems.Include(x => x.IndividualFashionItem).Where(x =>
                    x.IndividualFashionItem != null && x.IndividualFashionItem.Status == FashionItemStatus.Refundable &&
                    x.RefundExpirationDate < DateTime.UtcNow)
                .Select(x => x.IndividualFashionItem)
                .ToListAsync();

            foreach (var refundableItem in refundableItems)
            {
                if (refundableItem == null)
                {
                    throw new FashionItemNotFoundException();
                }

                refundableItem.Status = FashionItemStatus.Sold;


                dbContext.IndividualFashionItems.Update(refundableItem);
            }

            await dbContext.SaveChangesAsync();
        }

        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
}