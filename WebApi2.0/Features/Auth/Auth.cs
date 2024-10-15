using FastEndpoints;

namespace WebApi2._0.Features.Auth;

public sealed class Auth : Group
{
    public Auth()
    {
        Configure("auth", ep => { ep.Description(x => x.WithTags("Auth")); });
    }
}