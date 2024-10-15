using FastEndpoints;

namespace WebApi2._0.Features.Accounts;

public sealed class Accounts : Group
{
   public Accounts()
   {
      Configure("accounts", ep =>
      {
         ep.Description(x=>x.WithTags("Accounts"));
      });
   }
}