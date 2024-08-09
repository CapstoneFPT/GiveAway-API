using System.Text.Json.Serialization;

namespace BusinessObjects.Dtos.Commons;

public class GHNShop
{
    [JsonPropertyName("_id")] public int Id { get; set; }
    public string Name { get; set; }
    public string Phone { get; set; }
    public string Address { get; set; }
    [JsonPropertyName("ward_code")] public string WardCode { get; set; }
    [JsonPropertyName("district_id")] public int DistrictId { get; set; }
    public int Status { get; set; }
}