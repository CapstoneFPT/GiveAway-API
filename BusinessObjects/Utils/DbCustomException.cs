

namespace BusinessObjects.Utils;

public class DbCustomException : Exception
{
    public string AdditionalInfo { get; set; }
    public string Type { get; set; }
    public string Detail { get; set; }
    public string Title { get; set; }
    public string Instance { get; set; }
    public DbCustomException(string instance, string? innerExceptionMessage)
    {
        Type = "db-custom-exception";
        Detail = innerExceptionMessage ?? "Something went wrong with the database operations";
        Title = "Custom Database Exception";
        AdditionalInfo = "Maybe you can try again in a bit?";
        Instance = instance;
    }
}
