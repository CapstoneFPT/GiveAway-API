using BusinessObjects.Dtos.Commons;
using BusinessObjects.Entities;
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

    private  async Task CheckAndChangeToSoldRefundableItems()
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var fashionItemService = scope.ServiceProvider.GetRequiredService<IFashionItemService>();
            
            List<FashionItem> refundableItems = await fashionItemService.GetRefundableItems();
            await fashionItemService.ChangeToSoldItems(refundableItems);
        }
        
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
}