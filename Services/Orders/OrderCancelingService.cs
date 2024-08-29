using BusinessObjects.Dtos.Commons;
using BusinessObjects.Entities;
using BusinessObjects.Utils;
using Dao;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Services.OrderLineItems;

namespace Services.Orders;

public class OrderCancelingService : BackgroundService
{
    private readonly ILogger<OrderCancelingService> _logger;
    private readonly IServiceProvider _serviceProvider;
    private const int CheckInterval = 1000 * 60 * 10;

    public OrderCancelingService(IServiceProvider serviceProvider, ILogger<OrderCancelingService> logger)
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
                await CheckAndCancelOrder();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Order canceling service error");
            }

            await Task.Delay(CheckInterval, stoppingToken);
        }
    }

    private async Task CheckAndCancelOrder()
    {
        try
        {
            await using var scope = _serviceProvider.CreateAsyncScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<GiveAwayDbContext>();

            var ordersToCancel = await dbContext.Orders.Where(x =>
                x.CreatedDate < DateTime.UtcNow.AddDays(-1)
                && x.Status == OrderStatus.AwaitingPayment
                && x.PaymentMethod != PaymentMethod.COD).ToListAsync();

            foreach (var order in ordersToCancel)
            {
                order.Status = OrderStatus.Cancelled;
                dbContext.Orders.Update(order);

                var orderDetails = await dbContext.OrderLineItems.Include(x => x.IndividualFashionItem)
                    .Where(x => x.OrderId == order.OrderId).ToListAsync();

                foreach (var orderDetail in orderDetails)
                {
                    if (orderDetail.IndividualFashionItem == null)
                    {
                        throw new FashionItemNotFoundException();
                    }
                    orderDetail.IndividualFashionItem.Status = FashionItemStatus.Available;
                    dbContext.OrderLineItems.Update(orderDetail);
                }
            }

            await dbContext.SaveChangesAsync();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Order canceling service error");
        }
    }
}