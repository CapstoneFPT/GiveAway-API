using FastEndpoints;

namespace WebApi2._0.Features.Shops;

public sealed class Shops : Group
{
    public Shops()
    {
        Configure("shops", ep => { });
    }
}