using FastEndpoints;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApi2._0.Domain.Enums;
using WebApi2._0.Infrastructure.ExternalServices.VNPay;
using WebApi2._0.Infrastructure.Persistence;

namespace WebApi2._0.Features.Accounts.Orders.PurchaseOrder.Banking;

public class PurchaseOrderBankingEndpoint : Endpoint<PurchaseOrderRequest,
    Results<Ok<VnPayPurchaseResponse>,
        Conflict<ErrorResponse>,
        BadRequest<ErrorResponse>,
        UnauthorizedHttpResult,
        NotFound<ErrorResponse>
    >>
{
    private readonly IVnPayService _vnPayService;
    private readonly GiveAwayDbContext _dbContext;
    private readonly IConfiguration _configuration;

    public PurchaseOrderBankingEndpoint(IVnPayService vnPayService, GiveAwayDbContext dbContext,
        IConfiguration configuration)
    {
        _vnPayService = vnPayService;
        _dbContext = dbContext;
        _configuration = configuration;
    }

    public override void Configure()
    {
        Get("{orderId}/pay/vnpay");
        Claims("AccountId");
        Group<Features.Orders.Orders>();
    }

    public override async
        Task<Results<
            Ok<VnPayPurchaseResponse>,
            Conflict<ErrorResponse>,
            BadRequest<ErrorResponse>,
            UnauthorizedHttpResult,
            NotFound<ErrorResponse>>> ExecuteAsync(PurchaseOrderRequest req,
            CancellationToken ct)
    {
        var orderId = Route<Guid>("orderId");
        var order = await _dbContext.Orders.Where(x => x.OrderId == orderId)
            .Select(x => new
            {
                x.OrderId,
                x.TotalPrice,
                x.PaymentMethod,
                x.Status,
                x.MemberId
            })
            .FirstOrDefaultAsync(ct);

        if (order == null)
        {
            return TypedResults.NotFound(new ErrorResponse([
                    new ValidationFailure("OrderId", "Order not found")
                ])
            );
        }

        if (order.PaymentMethod != PaymentMethod.Banking)
        {
            return TypedResults.BadRequest(new ErrorResponse([
                    new ValidationFailure("PaymentMethod", "Payment method must be banking")
                ])
            );
        }

        if (order.Status != OrderStatus.AwaitingPayment)
        {
            return TypedResults.BadRequest(
                new ErrorResponse([
                    new ValidationFailure("Status", "Order is not awaiting payment")
                ])
            );
        }

        if (order.MemberId != req.AccountId)
        {
            return TypedResults.Unauthorized();
        }

        var redirectUrl = req.RedirectUrl;
        var paymentUrl = _vnPayService.CreatePaymentUrl(
            order.OrderId,
            order.TotalPrice,
            $"{orderId}", "orders", redirectUrl);

        return TypedResults.Ok(new VnPayPurchaseResponse()
        {
            PaymentUrl = paymentUrl
        });
    }
}

public record VnPayPurchaseResponse
{
    public string PaymentUrl { get; set; }
}

public record PurchaseOrderRequest
{
    [FromClaim("AccountId")] public Guid AccountId { get; set; }

    ///<summary>
    ///This is the URL to redirect after the payment is completed. Make sure to only pass the host URL.
    /// </summary>
    [FromQuery]
    public required string RedirectUrl { get; set; }
}