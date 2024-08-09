namespace BusinessObjects.Dtos.Commons;

public class GHNDistrictResponse
{
    public int DistrictId { get; set; }
    public int ProvinceId { get; set; }
    public string DistrictName { get; set; }
    public int SupportType { get; set; }
    public string[] NameExtension { get; set; }
    public bool CanUpdateCOD { get; set; }
    public int Status { get; set; }
}