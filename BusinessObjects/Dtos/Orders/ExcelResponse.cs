namespace BusinessObjects.Dtos.Orders;

public class ExcelResponse
{
    public byte[] Content { get; set; }
    public string FileName { get; set; }
}

public class ExportOrdersRequest
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}