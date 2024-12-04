using FastEndpoints;

namespace WebApi2._0.Features.Orders;

public sealed class Orders : Group
{
    public Orders()
    {
        Configure("orders", ep => { });
    }
}