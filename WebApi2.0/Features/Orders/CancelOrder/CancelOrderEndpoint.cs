using FastEndpoints;
using FluentValidation.Results;
using LinqKit;
using Microsoft.EntityFrameworkCore;
using WebApi2._0.Common;
using WebApi2._0.Domain.Entities;
using WebApi2._0.Domain.Enums;
using WebApi2._0.Infrastructure.Persistence;

namespace WebApi2._0.Features.Orders.CancelOrder;

public class CancelOrderEndpoint : EndpointWithoutRequest
{
    private readonly GiveAwayDbContext _dbContext;

    public CancelOrderEndpoint(GiveAwayDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public override void Configure()
    {
        Patch("orders/{orderId}/cancel");
        Group<Orders>();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var orderId = Route<Guid>("orderId");
        var order = await _dbContext.Orders
            .Include(x => x.OrderLineItems)
            .Include(x => x.Member)
            .FirstOrDefaultAsync(x => x.OrderId == orderId, ct);

        if (order == null)
        {
            await SendNotFoundAsync(ct);
            return;
        }

        if (!(order.Status.Equals(OrderStatus.Pending) || order.Status.Equals(OrderStatus.AwaitingPayment)))
        {
            await SendAsync(new ErrorResponse([
                new ValidationFailure("OrderId", "Order is not pending or awaiting payment")
            ]), cancellation: ct);
        }

        if (!order.PaymentMethod.Equals(PaymentMethod.COD))
        {
            if (order.Member == null)
            {
                throw new OrderMissingMemberException();
            }

            order.Member.Balance += order.TotalPrice;

            var adminAccount =
                await _dbContext.Accounts.FirstOrDefaultAsync(x => x.Role == Domain.Enums.Roles.Admin, ct);

            if (adminAccount == null)
            {
                throw new AdminAccountNotFoundException();
            }

            order.Status = OrderStatus.Cancelled;
            order.OrderLineItems.ForEach(x=>x.IndividualFashionItem.Status = FashionItemStatus.Available);
            _dbContext.Orders.Update(order);
            
            adminAccount.Balance += order.TotalPrice;
            _dbContext.Accounts.Update(adminAccount);

            var transaction = new Transaction()
            {
                OrderId = orderId,
                ReceiverId = order.MemberId,
                ReceiverBalance = order.Member.Balance,
                SenderBalance = adminAccount.Balance,
                SenderId = adminAccount.AccountId,
                Amount = order.TotalPrice,
                CreatedDate = DateTime.UtcNow,
                Type = TransactionType.RefundProduct,
                PaymentMethod = PaymentMethod.Point
            };
            await _dbContext.Transactions.AddAsync(transaction,ct);
            
            await _dbContext.SaveChangesAsync(ct);

            await SendOkAsync(ct);
        }
    }
}

public class OrderMissingMemberException : Exception
{
}
