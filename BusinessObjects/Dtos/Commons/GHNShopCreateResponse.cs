using System.Text.Json.Serialization;

namespace BusinessObjects.Dtos.Commons;

public  class GHNShopCreateResponse
{
    [JsonPropertyName("shop_id")] public int ShopId { get; set; }
}