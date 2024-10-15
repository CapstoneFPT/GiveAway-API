using FastEndpoints;
using FluentValidation.Results;
using LinqKit;
using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Asn1.Ocsp;
using Serilog;
using WebApi2._0.Common;
using WebApi2._0.Domain.Entities;
using WebApi2._0.Domain.Enums;
using WebApi2._0.Infrastructure.ExternalServices.VNPay;
using WebApi2._0.Infrastructure.Persistence;
using Order = WebApi2._0.Domain.Entities.Order;

namespace WebApi2._0.Features.Accounts.Orders.PurchaseOrder.Banking;

public class PurchaseOrderBankingPaymentReturnEndpoint : EndpointWithoutRequest
{
    private readonly IVnPayService _vnPayService;
    private readonly GiveAwayDbContext _dbContext;
    private readonly IConfiguration _configuration;

    public PurchaseOrderBankingPaymentReturnEndpoint(IVnPayService vnPayService, GiveAwayDbContext dbContext,
        IConfiguration configuration)
    {
        _vnPayService = vnPayService;
        _dbContext = dbContext;
        _configuration = configuration;
    }

    public override void Configure()
    {
        Get("payment-return");
        Group<Features.Orders.Orders>();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var redirectUrl = _configuration["RedirectUrl"] + "process-payment";
        try
        {
            var requestParams = HttpContext.Request.Query;
            var bankingPaymentResponse = _vnPayService.ProcessPayment(requestParams);

            if (bankingPaymentResponse.Success)
            {
                var order = await _dbContext.Orders.Include(x => x.OrderLineItems)
                    .ThenInclude(x => x.IndividualFashionItem)
                    .FirstOrDefaultAsync(
                        x => x.OrderId == new Guid(bankingPaymentResponse.OrderId), ct);
                if (order == null)
                {
                    await SendRedirectAsync(
                        WebRedirectionUtils.CreatePaymentRedirectUrlForMainPage(redirectUrl, PaymentStatus.Error,
                            "Order not found"));
                    return;
                }

                if (order.Status != OrderStatus.AwaitingPayment)
                {
                    await SendRedirectAsync(WebRedirectionUtils.CreatePaymentRedirectUrlForMainPage(redirectUrl,
                        PaymentStatus.Error, "Order is not awaiting payment"));
                    return;
                }

                await CreateTransaction(bankingPaymentResponse, order);

                order.Status = OrderStatus.Pending;
                foreach (var orderOrderLineItem in order.OrderLineItems)
                {
                    orderOrderLineItem.PaymentDate = DateTime.UtcNow;
                }

                order.OrderLineItems.ForEach(x => x.IndividualFashionItem.Status = FashionItemStatus.PendingForOrder);

                _dbContext.Orders.Update(order);

                await _dbContext.SaveChangesAsync(ct);

                await SendRedirectAsync(WebRedirectionUtils.CreatePaymentRedirectUrlForMainPage(redirectUrl,
                    PaymentStatus.Success,
                    "Payment success"));
                return;
            }

            await SendRedirectAsync(WebRedirectionUtils.CreatePaymentRedirectUrlForMainPage(redirectUrl,
                PaymentStatus.Error,
                "Payment failed"));
        }
        catch (Exception e)
        {
            Log.Fatal(e, "Payment failed");
            await SendRedirectAsync(
                WebRedirectionUtils.CreatePaymentRedirectUrlForMainPage(redirectUrl, PaymentStatus.Error,
                    "Payment failed"));
        }
    }

    private async Task<Transaction> CreateTransaction(VnPaymentResponse paymentResponse,
        Order order)
    {
        var adminAccount = await _dbContext.Accounts.FirstOrDefaultAsync(x => x.Role == Domain.Enums.Roles.Admin);


        var memberAccount = await _dbContext.Accounts.FirstOrDefaultAsync(x => x.AccountId == order.MemberId);
        var transaction = new Transaction()
        {
            OrderId = new Guid(paymentResponse.OrderId),
            CreatedDate = DateTime.UtcNow,
            Amount = order.TotalPrice,
            VnPayTransactionNumber = paymentResponse.TransactionId,
            SenderId = order.MemberId,
            SenderBalance = memberAccount.Balance,
            ReceiverId = adminAccount.AccountId,
            ReceiverBalance = adminAccount.Balance,
            Type = TransactionType.Purchase,
            PaymentMethod = PaymentMethod.Banking, TransactionCode = CodeGenerationUtils.GenerateCode("TRX")
        };

        await _dbContext.Transactions.AddAsync(transaction);
        await _dbContext.SaveChangesAsync();

        return transaction;
    }
}