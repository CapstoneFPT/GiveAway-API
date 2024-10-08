namespace WebApi2._0.Common;

public static class CodeGenerationUtils
{
    public static string GenerateCode(string prefix)
    {
        var random = new Random();
        string timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
        string randomString = random.Next(1000, 9999).ToString();
        return $"{prefix}-{timestamp}-{randomString}";
    }
}