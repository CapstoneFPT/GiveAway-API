namespace WebApi2._0.Common;

public enum PaymentStatus
{
   Success,
   Error
}
public static class WebRedirectionUtils
{
   public static string CreatePaymentRedirectUrlForMainPage(string baseUrl, PaymentStatus status, string message)
   {
      return $"{baseUrl}?paymentstatus={status.ToString().ToLower()}&message={Uri.EscapeDataString(message)}";
   }  
}