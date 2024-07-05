using BusinessObjects.Dtos.Commons;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace Services.VnPayService;

public class VnPayService : IVnPayService
{
    private readonly VnPayLibrary _vnPayLibrary;
    private readonly VnPaySettings _vnPaySettings;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public VnPayService( IOptions<VnPaySettings> vnPaySettings, IHttpContextAccessor httpContextAccessor)
    {
        _vnPayLibrary = new VnPayLibrary();
        _vnPaySettings = vnPaySettings.Value;
        _httpContextAccessor = httpContextAccessor;
    }

    public string CreatePaymentUrl(string orderId, long amount, string orderInfo)
    {
        _vnPayLibrary.AddRequestData("vnp_Version", VnPayLibrary.VERSION);
        _vnPayLibrary.AddRequestData("vnp_Command", "pay");
        _vnPayLibrary.AddRequestData("vnp_TmnCode", _vnPaySettings.TmnCode);
        _vnPayLibrary.AddRequestData("vnp_Amount", (amount * 100).ToString());
        _vnPayLibrary.AddRequestData("vnp_CreateDate", DateTime.Now.ToString("yyyyMMddHHmmss"));
        _vnPayLibrary.AddRequestData("vnp_CurrCode", "VND");
        _vnPayLibrary.AddRequestData("vnp_IpAddr", Utils.GetIpAddress(_httpContextAccessor.HttpContext));
        _vnPayLibrary.AddRequestData("vnp_Locale", "vn");
        _vnPayLibrary.AddRequestData("vnp_OrderInfo", orderInfo);
        _vnPayLibrary.AddRequestData("vnp_OrderType", "other");
        _vnPayLibrary.AddRequestData("vnp_ReturnUrl", _vnPaySettings.ReturnUrl);
        _vnPayLibrary.AddRequestData("vnp_TxnRef", orderId);
        
        var result =  _vnPayLibrary.CreateRequestUrl(_vnPaySettings.PaymentUrl, _vnPaySettings.HashSecret);
        return result;
    }

    public VnPaymentResponseModel ProcessPayment(IQueryCollection collection)
    {
        foreach (var (key,value) in collection)
        {
            if (!string.IsNullOrEmpty(key) && key.StartsWith("vnp_"))
            {
               _vnPayLibrary.AddResponseData(key, value); 
            } 
        }
        
        var orderId = _vnPayLibrary.GetResponseData("vnp_TxnRef");
        var vnp_TransactionNo = _vnPayLibrary.GetResponseData("vnp_TransactionNo");
        var vnp_ResponseCode = _vnPayLibrary.GetResponseData("vnp_ResponseCode");
        var vnp_SecureHash = collection.FirstOrDefault(x=>x.Key == "vnp_SecureHash").Value;
        
        bool isValidSignature = _vnPayLibrary.ValidateSignature(
            vnp_SecureHash, _vnPaySettings.HashSecret);

        if (isValidSignature)
        {
            return new VnPaymentResponseModel
            {
                Success = vnp_ResponseCode == "00",
                PaymentMethod = "VnPay",
                OrderDescription = _vnPayLibrary.GetResponseData("vnp_OrderInfo"),
                OrderId = orderId,
                PaymentId = vnp_TransactionNo,
                TransactionId = vnp_TransactionNo,
                Token = vnp_SecureHash,
                VnPayResponseCode = vnp_ResponseCode
            };
        } 
        return new VnPaymentResponseModel
        {
            Success = false,
            PaymentMethod = "VnPay",
            OrderId = orderId,
            PaymentId = vnp_TransactionNo,
            TransactionId = vnp_TransactionNo,
            Token = vnp_SecureHash,
            VnPayResponseCode = "97"
        };
    }
}