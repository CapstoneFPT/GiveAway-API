namespace BusinessObjects.Dtos.Commons;

public class GHNProvinceResponse
{
    public int ProvinceId { get; set; }
    public string ProvinceName { get; set; }
    public string[] NameExtension { get; set; } = [];
    public bool CanUpdateCod { get; set; }
    public int Status { get; set; }
}