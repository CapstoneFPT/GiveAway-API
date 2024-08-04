using BusinessObjects.Dtos.Commons;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Services.VnPayService;

public class VnPayService : IVnPayService
{
    private readonly VnPayLibrary _vnPayLibrary;
    private readonly VnPaySettings _vnPaySettings;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IConfiguration _configuration;
    private readonly IHostEnvironment _environment;

    public VnPayService( IOptions<VnPaySettings> vnPaySettings, IHttpContextAccessor httpContextAccessor, IConfiguration configuration, IHostEnvironment environment)
    {
        _vnPayLibrary = new VnPayLibrary();
        _vnPaySettings = vnPaySettings.Value;
        _httpContextAccessor = httpContextAccessor;
        _configuration = configuration;
        _environment = environment;
    }

    public string CreatePaymentUrl(Guid orderId, decimal amount, string orderInfo, string resourceName)
    {
        var tick = DateTime.Now.Ticks.ToString();
        var createdDate = DateTime.Now.ToString("yyyyMMddHHmmss");
        var expiredDate = DateTime.Now.AddMinutes(15).ToString("yyyyMMddHHmmss");

        if (_environment.IsProduction())
        {
            // Vietnam Time
            createdDate = DateTime.Now.AddHours(7).ToString("yyyyMMddHHmmss");
            expiredDate = DateTime.Now.AddHours(7).AddMinutes(15).ToString("yyyyMMddHHmmss");
        }
        
        _vnPayLibrary.AddRequestData("vnp_Version", VnPayLibrary.VERSION);
        _vnPayLibrary.AddRequestData("vnp_Command", "pay");
        _vnPayLibrary.AddRequestData("vnp_TmnCode", _vnPaySettings.TmnCode);
        _vnPayLibrary.AddRequestData("vnp_Amount", (amount * 100).ToString());
        _vnPayLibrary.AddRequestData("vnp_CreateDate", createdDate);
        _vnPayLibrary.AddRequestData("vnp_CurrCode", "VND");
        _vnPayLibrary.AddRequestData("vnp_IpAddr", Utils.GetIpAddress(_httpContextAccessor.HttpContext) != null ? Utils.GetIpAddress(_httpContextAccessor.HttpContext) : "172.18.0.3");
        _vnPayLibrary.AddRequestData("vnp_Locale", "vn");
        _vnPayLibrary.AddRequestData("vnp_OrderInfo", orderId.ToString());
        _vnPayLibrary.AddRequestData("vnp_OrderType", "other");
        _vnPayLibrary.AddRequestData("vnp_ExpireDate", expiredDate);
        _vnPayLibrary.AddRequestData("vnp_ReturnUrl", $"{_httpContextAccessor.HttpContext.Request.Scheme}://{_httpContextAccessor.HttpContext.Request.Host}/api/{resourceName}/payment-return");
        _vnPayLibrary.AddRequestData("vnp_TxnRef", tick);
        
        var result =  _vnPayLibrary.CreateRequestUrl(_vnPaySettings.PaymentUrl, _vnPaySettings.HashSecret);
        return result;
    }

    public VnPaymentResponse ProcessPayment(IQueryCollection collection)
    {
        foreach (var (key,value) in collection)
        {
            if (!string.IsNullOrEmpty(key) && key.StartsWith("vnp_"))
            {
               _vnPayLibrary.AddResponseData(key, value.ToString()); 
            } 
        }
        
        var transactionRef = _vnPayLibrary.GetResponseData("vnp_TxnRef");
        var vnp_TransactionNo = _vnPayLibrary.GetResponseData("vnp_TransactionNo");
        var vnp_ResponseCode = _vnPayLibrary.GetResponseData("vnp_ResponseCode");
        var vnp_SecureHash = collection.FirstOrDefault(x=>x.Key == "vnp_SecureHash").Value;
        var vnp_OrderInfo = _vnPayLibrary.GetResponseData("vnp_OrderInfo");
        
        bool isValidSignature = _vnPayLibrary.ValidateSignature(
            vnp_SecureHash!, _vnPaySettings.HashSecret);

        if (isValidSignature)
        {
            return new VnPaymentResponse
            {
                Success = vnp_ResponseCode == "00",
                PaymentMethod = "VnPay",
                OrderDescription = $"{vnp_OrderInfo} - {transactionRef} - {vnp_TransactionNo}",
                OrderId = vnp_OrderInfo,
                PaymentId = vnp_TransactionNo,
                TransactionId = vnp_TransactionNo,
                Token = vnp_SecureHash!,
                VnPayResponseCode = vnp_ResponseCode
            };
        } 
        return new VnPaymentResponse
        {
            Success = false,
        };
    }
}