using BusinessObjects.Dtos.Commons;
using Microsoft.AspNetCore.Http;

namespace Services.VnPayService;

public interface IVnPayService
{
    VnPaymentResponseModel ProcessPayment(IQueryCollection collection);
    string CreatePaymentUrl(string orderId, long amount, string orderInfo);
}