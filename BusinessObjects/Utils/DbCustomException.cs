

namespace BusinessObjects.Utils;

public class DbCustomException : Exception
{
    public string AdditionalInfo { get; set; }
    public string Type { get; set; }
    public string Detail { get; set; }
    public string Title { get; set; }
    public string Instance { get; set; }
    public DbCustomException(string instance)
    {
        Type = "db-custom-exception";
        Detail = "Something went wrong while interacting with the database";
        Title = "Custom Database Exception";
        AdditionalInfo = "Maybe you can try again in a bit?";
        Instance = instance;
    }
}
