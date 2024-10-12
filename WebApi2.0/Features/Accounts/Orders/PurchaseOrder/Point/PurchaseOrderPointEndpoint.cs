using FastEndpoints;
using LinqKit;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using WebApi2._0.Common;
using WebApi2._0.Domain.Entities;
using WebApi2._0.Domain.Enums;
using WebApi2._0.Features.Accounts.Orders.PurchaseOrder.Banking;
using WebApi2._0.Infrastructure.Persistence;
using Order = WebApi2._0.Domain.Entities.Order;

namespace WebApi2._0.Features.Accounts.Orders.PurchaseOrder.Point;

public class PurchaseOrderPointEndpoint : Endpoint<PurchaseOrderRequest,
    Results<Ok, BadRequest<ErrorResponse>, UnauthorizedHttpResult, NotFound,ProblemHttpResult>>
{
    private readonly GiveAwayDbContext _dbContext;

    public PurchaseOrderPointEndpoint(GiveAwayDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public override void Configure()
    {
        Put("api/orders/{orderId}/pay/points");
        Claims("AccountId");
    }

    public override async Task<Results<Ok, BadRequest<ErrorResponse>, UnauthorizedHttpResult, NotFound, ProblemHttpResult>>
        ExecuteAsync(
            PurchaseOrderRequest req, CancellationToken ct)
    {
        var orderId = Route<Guid>("orderId");
        var order = await _dbContext.Orders.Include(x => x.Member)
            .Include(x => x.OrderLineItems)
            .ThenInclude(x => x.IndividualFashionItem)
            .FirstOrDefaultAsync(x =>
                    x.OrderId == orderId, ct
            );
        var adminAccount = await _dbContext.Accounts.FirstOrDefaultAsync(x => x.Role == Domain.Enums.Roles.Admin, ct);

        if (order == null)
        {
            return TypedResults.NotFound();
        }

        if (adminAccount == null)
        {
            throw new AdminAccountNotFoundException();
        }

        if (req.AccountId != order.MemberId)
        {
            return TypedResults.Unauthorized();
        }

        if (order.PaymentMethod != PaymentMethod.Point)
        {
            return TypedResults.BadRequest(new ErrorResponse()
            {
                Errors = new Dictionary<string, List<string>>()
                {
                    ["PaymentMethod"] = ["Only point payment method is available"]
                },
                Message = "Invalid payment method",
                StatusCode = 400
            });
        }

        if (order.Status != OrderStatus.AwaitingPayment)
        {
            return TypedResults.BadRequest(new ErrorResponse()
            {
                Errors = new Dictionary<string, List<string>>()
                {
                    ["Status"] = ["Only awaiting payment status is available"]
                },
                Message = "Invalid status",
                StatusCode = 400
            });
        }

        if (order.Member == null || order.PurchaseType != PurchaseType.Online)
        {
            return TypedResults.BadRequest(new ErrorResponse()
            {
                Errors = new Dictionary<string, List<string>>()
                {
                    ["PurchaseType"] = ["Only online purchase type is supported"],
                    ["Member"] = ["Only orders with an associated account is supported"]
                },
                Message = "Invalid purchase type",
                StatusCode = 400
            });
        }

        if (order.Member.Balance < order.TotalPrice)
        {
            return TypedResults.BadRequest(new ErrorResponse()
            {
                Errors = new Dictionary<string, List<string>>()
                {
                    ["Balance"] = ["Balance is not enough"]
                },
                Message = "Balance is not enough",
                StatusCode = 400
            });
        }


        order.OrderLineItems.ForEach(x => x.PaymentDate = DateTime.UtcNow);
        order.Status = OrderStatus.Pending;
        order.OrderLineItems.ForEach(x => x.IndividualFashionItem.Status = FashionItemStatus.PendingForOrder);
        order.Member.Balance -= order.TotalPrice;

        _dbContext.Orders.Update(order);

        adminAccount.Balance += order.TotalPrice;
        _dbContext.Accounts.Update(adminAccount);


        var transaction = new Transaction()
        {
            OrderId = order.OrderId,
            CreatedDate = DateTime.UtcNow,
            Amount = order.TotalPrice,
            SenderId = order.MemberId,
            SenderBalance = order.Member.Balance,
            ReceiverId = adminAccount.AccountId,
            ReceiverBalance = adminAccount.Balance,
            Type = TransactionType.Purchase,
            PaymentMethod = PaymentMethod.Point
        };
        
        await _dbContext.Transactions.AddAsync(transaction,ct);
        await _dbContext.SaveChangesAsync(ct);
        //TODO: Send email

        return TypedResults.Ok();
    }
}