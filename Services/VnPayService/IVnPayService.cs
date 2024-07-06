using BusinessObjects.Dtos.Commons;
using Microsoft.AspNetCore.Http;

namespace Services.VnPayService;

public interface IVnPayService
{
    VnPaymentResponse ProcessPayment(IQueryCollection collection);
    string CreatePaymentUrl(Guid orderId, long amount, string orderInfo);
}