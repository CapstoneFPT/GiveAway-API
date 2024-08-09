using System.Text.Json.Serialization;

namespace BusinessObjects.Dtos.Commons;

public class GHNShopListResponse
{
    [JsonPropertyName("last_offset")] public int LastOffset { get; set; }

    public List<GHNShop> Shops { get; set; } = [];
}