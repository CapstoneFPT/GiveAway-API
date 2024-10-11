namespace WebApi2._0.Infrastructure.ExternalServices.VNPay;

public class VnPaySettings
{
    public string TmnCode { get; set; }
    public string HashSecret { get; set; }
    public string PaymentUrl { get; set; }
    public string ReturnUrl { get; set; }
}