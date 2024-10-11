using System.Data.Entity;
using FastEndpoints;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http.HttpResults;
using WebApi2._0.Common;
using WebApi2._0.Domain.Entities;
using WebApi2._0.Domain.Enums;
using WebApi2._0.Infrastructure.Persistence;
using Order = WebApi2._0.Domain.Entities.Order;

namespace WebApi2._0.Features.Accounts.Orders.PlaceOrder;

public sealed class PlaceOrderEndpoint : Endpoint<PlaceOrderRequest,
    Results<CreatedAtRoute<PlaceOrderResponse>, BadRequest<ErrorResponse>>>
{
    private readonly GiveAwayDbContext _dbContext;

    public PlaceOrderEndpoint(GiveAwayDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public override void Configure()
    {
        Post("api/accounts/{accountId}/orders");
        Roles(Domain.Enums.Roles.Member.ToString());
    }

    public override async Task<Results<
        CreatedAtRoute<PlaceOrderResponse>, BadRequest<ErrorResponse>
    >> ExecuteAsync(
        PlaceOrderRequest req, CancellationToken ct)
    {
        var accountId = Route<Guid>("accountId");

        if (req.PaymentMethod.Equals(PaymentMethod.Cash))
        {
            var error = new ErrorResponse([
                new ValidationFailure("PaymentMethod", "Cash payment is not available")
            ]);
            return TypedResults.BadRequest(error);
        }

        if (req.CartItems.Count < 1)
        {
            var error = new ErrorResponse([
                new ValidationFailure("CartItems", "Cart is empty")
            ]);
            return TypedResults.BadRequest(error);
        }

        var unavailableItems = await GetUnavailableItems(req.CartItems);
        if (unavailableItems is { Count: > 0 })
        {
            var error = new ErrorResponse([
                new ValidationFailure("CartItems", "Some items are not available for ordering: " + unavailableItems)
            ]);
            return TypedResults.BadRequest(error);
        }

        var orderedItems = await GetOrderedItems(req.CartItems, accountId);
        if (orderedItems is { Count: > 0 })
        {
            var error = new ErrorResponse([
                new ValidationFailure("CartItems", "Some items are already ordered: " + orderedItems)
            ]);
            return TypedResults.BadRequest(error);
        }

        var result = await PlaceOrder(req, accountId);

        return TypedResults.CreatedAtRoute(result, "api/orders/{orderId}", result.OrderId);
    }

    private async Task<PlaceOrderResponse> PlaceOrder(PlaceOrderRequest req, Guid accountId)
    {
        var listItem = await _dbContext
            .IndividualFashionItems
            .Include(x => x.MasterItem)
            .Where(x =>
                req.CartItems.Contains(x.ItemId))
            .ToListAsync();

        var memberAccount = await _dbContext.Accounts
            .FirstOrDefaultAsync(c => c.AccountId == accountId);


        var order = new Order
        {
            MemberId = accountId,
            PaymentMethod = req.PaymentMethod,
            Address = req.Address,
            GhnDistrictId = req.GhnDistrictId,
            GhnWardCode = req.GhnWardCode,
            GhnProvinceId = req.GhnProvinceId,
            AddressType = req.AddressType,
            PurchaseType = PurchaseType.Online,
            RecipientName = req.RecipientName,
            ShippingFee = req.ShippingFee,
            Discount = req.Discount,
            Phone = req.Phone,
            Email = memberAccount.Email,
            Status = req.PaymentMethod.Equals(PaymentMethod.COD) ? OrderStatus.Pending : OrderStatus.AwaitingPayment,
            CreatedDate = DateTime.UtcNow,
            TotalPrice = listItem.Sum(c => c.SellingPrice!.Value) + req.ShippingFee - req.Discount,
            OrderCode = await GenerateUniqueString()
        };


        var orderLineItems = listItem.Select(x => new OrderLineItem()
        {
            UnitPrice = x.SellingPrice!.Value,
            CreatedDate = DateTime.UtcNow,
            Quantity = 1,
            IndividualFashionItemId = x.ItemId
        }).ToList();

        order.OrderLineItems = orderLineItems;

        _dbContext.Orders.Add(order);

        if (req.PaymentMethod.Equals(PaymentMethod.COD))
        {
            listItem.ForEach(i => i.Status = FashionItemStatus.PendingForOrder);
            _dbContext.IndividualFashionItems.UpdateRange(listItem);
        }

        await _dbContext.SaveChangesAsync();
        
        var orderResponse = new PlaceOrderResponse()
        {
            OrderId = order.OrderId,
            Quantity = order.OrderLineItems.Count,
            TotalPrice = order.TotalPrice,
            OrderCode = order.OrderCode,
            CreatedDate = order.CreatedDate,
            MemberId = order.MemberId,
            PaymentMethod = order.PaymentMethod,
            PurchaseType = order.PurchaseType,
            Address = order.Address,
            AddressType = order.AddressType,
            RecipientName = order.RecipientName,
            ContactNumber = order.Phone,
            CustomerName = memberAccount.Fullname,
            Email = order.Email,
            ShippingFee = order.ShippingFee,
            Discount = order.Discount,
            Status = order.Status,
        };
        return orderResponse;
    }


    private async Task<string> GenerateUniqueString()
    {
        for (int attempt = 0; attempt < 5; attempt++)
        {
            const string prefix = "GA-OD-";
            string code = CodeGenerationUtils.GenerateCode(prefix);
            bool isCodeExisted = await _dbContext.Recharges.AnyAsync(r => r.RechargeCode == code);

            if (!isCodeExisted)
            {
                return code;
            }

            await Task.Delay(100 * (int)Math.Pow(2, attempt));
        }

        throw new TimeoutException();
    }

    private async Task<List<Guid?>> GetOrderedItems(List<Guid> listItemId, Guid memberId)
    {
        var orderedItems = await _dbContext.OrderLineItems.Where(x =>
                x.Order.MemberId == memberId && x.Order.Status == OrderStatus.AwaitingPayment)
            .Where(x => listItemId.Contains(x.IndividualFashionItemId ?? Guid.Empty))
            .Select(x => x.IndividualFashionItemId)
            .ToListAsync();
        return orderedItems;
    }

    private async Task<List<Guid>> GetUnavailableItems(List<Guid> listItemId)
    {
        var availableItems = await _dbContext.IndividualFashionItems
            .Where(x => listItemId.Contains(x.ItemId))
            .Where(x => x.Status == FashionItemStatus.Available || x.Status == FashionItemStatus.Reserved)
            .Select(x => x.ItemId)
            .ToListAsync();

        var unavailableItems = listItemId.Except(availableItems).ToList();
        return unavailableItems;
    }
}