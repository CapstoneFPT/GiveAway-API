﻿using BusinessObjects.Dtos.Commons;
using BusinessObjects.Entities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Services.OrderDetails;

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
            using var scope = _serviceProvider.CreateScope();
            var orderService = scope.ServiceProvider.GetRequiredService<IOrderService>();
            var orderDetailService = scope.ServiceProvider.GetRequiredService<IOrderDetailService>();

            var ordersToCancel = await orderService.GetOrdersToCancel();
            orderService.CancelOrders(ordersToCancel);

            foreach (var order in ordersToCancel)
            {
               var orderDetails = await orderService.GetOrderDetailByOrderId(order!.OrderId); 
               await orderDetailService.ChangeFashionItemsStatus(orderDetails, FashionItemStatus.Available);
            } 
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Order canceling service error");
        }
    }
}