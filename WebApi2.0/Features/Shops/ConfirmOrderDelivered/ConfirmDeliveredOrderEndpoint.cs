using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using Quartz;
using WebApi2._0.Domain.Entities;
using WebApi2._0.Domain.Enums;
using WebApi2._0.Infrastructure.Persistence;
using Order = WebApi2._0.Domain.Entities.Order;

namespace WebApi2._0.Features.Shops.ConfirmOrderDelivered;

public class ConfirmDeliveredOrderEndpoint : EndpointWithoutRequest
{
    private readonly GiveAwayDbContext _dbContext;
    private readonly ISchedulerFactory _schedulerFactory;

    public ConfirmDeliveredOrderEndpoint(GiveAwayDbContext dbContext, ISchedulerFactory schedulerFactory)
    {
        _dbContext = dbContext;
        _schedulerFactory = schedulerFactory;
    }

    public override void Configure()
    {
        Put("{shopId}/orders/{orderId}/confirm-deliveried");
        Group<Shops>();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var orderId = Route<Guid>("orderId");
        var shopId = Route<Guid>("shopId");

        var order = await GetOrderWithDetails(orderId, ct);
        if (order is null)
        {
            await SendNotFoundAsync(ct);
            return;
        }

        var orderLineItemsFromShop = GetOrderLineItemsForShop(order, shopId);
        if (orderLineItemsFromShop.Count == 0)
        {
            throw new EmptyOrderException();
        }

        await ProcessOrderLineItems(orderLineItemsFromShop, ct);

        await UpdateOrderStatus(order, ct);

        await _dbContext.SaveChangesAsync(ct);
    }

    private async Task<Order?> GetOrderWithDetails(Guid orderId, CancellationToken ct)
    {
        return await _dbContext.Orders
            .Include(x => x.OrderLineItems)
                .ThenInclude(x => x.IndividualFashionItem)
                    .ThenInclude(x => x.MasterItem)
            .Include(x => x.Member)
            .FirstOrDefaultAsync(x => x.OrderId == orderId, ct);
    }

    private static List<OrderLineItem> GetOrderLineItemsForShop(Order order, Guid shopId)
    {
        return order.OrderLineItems
            .Where(x => x.IndividualFashionItem.MasterItem.ShopId == shopId)
            .ToList();
    }

    private async Task ProcessOrderLineItems(List<OrderLineItem> orderLineItems, CancellationToken ct)
    {
        foreach (var orderLineItem in orderLineItems)
        {
            if (orderLineItem.IndividualFashionItem.Status != FashionItemStatus.OnDelivery)
            {
                throw new FashionItemNotFoundException();
            }

            orderLineItem.IndividualFashionItem.Status = FashionItemStatus.Refundable;
            orderLineItem.RefundExpirationDate = DateTime.UtcNow.AddMinutes(15);

            if (orderLineItem.Order.PaymentMethod == PaymentMethod.COD)
            {
                orderLineItem.PaymentDate = DateTime.UtcNow;
            }

            await ScheduleRefundableItemExpiration(orderLineItem.IndividualFashionItem.ItemId, orderLineItem.RefundExpirationDate.Value);
        }
    }

    private async Task UpdateOrderStatus(Order order, CancellationToken ct)
    {
        if (order.OrderLineItems.All(x => x.IndividualFashionItem.Status == FashionItemStatus.Refundable))
        {
            order.Status = OrderStatus.Completed;
            order.CompletedDate = DateTime.UtcNow;

            if (order.PaymentMethod == PaymentMethod.COD)
            {
                await CreateCodTransaction(order, ct);
            }
        }
    }

    private async Task CreateCodTransaction(Order order, CancellationToken ct)
    {
        var transaction = new Transaction
        {
            OrderId = order.OrderId,
            CreatedDate = DateTime.UtcNow,
            PaymentMethod = PaymentMethod.COD,
            SenderId = order.MemberId,
            Amount = order.TotalPrice,
            SenderBalance = order.Member?.Balance ?? 0,
            Type = TransactionType.Purchase,
        };
        await _dbContext.Transactions.AddAsync(transaction, ct);
    }

    private async Task ScheduleRefundableItemExpiration(Guid itemId, DateTime refundExpirationDate)
    {
        var schedule = await _schedulerFactory.GetScheduler();
        var jobDataMap = new JobDataMap()
        {
            { "RefundItemId", itemId }
        };
        var endJob = JobBuilder.Create<FashionItemRefundExpirationJob>()
            .WithIdentity($"EndRefundableItem_{itemId}")
            .SetJobData(jobDataMap)
            .Build();
        var endTrigger = TriggerBuilder.Create()
            .WithIdentity($"EndRefundableItemTrigger_{itemId}")
            .StartAt(new DateTimeOffset(refundExpirationDate))
            .Build();
        await schedule.ScheduleJob(endJob, endTrigger);
    }
}

public class FashionItemNotFoundException : Exception
{
}

public class EmptyOrderException : Exception
{
}
