using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace BusinessObjects.Dtos.Commons;

public class GHNShopCreateRequest
{
    [JsonPropertyName("district_id")]
    [Required]
    public int DistrictId { get; set; }

    [JsonPropertyName("ward_code")]
    [Required]
    public string WardCode { get; set; }

    [Phone] [Required] public string Phone { get; set; }
    [Required] public string Address { get; set; }
}