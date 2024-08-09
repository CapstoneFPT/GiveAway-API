namespace BusinessObjects.Dtos.Commons;

public class GHNWardResponse
{
    public int DistrictId { get; set; }
    public int WardCode { get; set; }
    public string WardName { get; set; }
    public string[] NameExtension { get; set; }
    public bool CanUpdateCod { get; set; }
    public int SupportType { get; set; }
    public int Status { get; set; }
}