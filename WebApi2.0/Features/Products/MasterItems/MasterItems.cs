using FastEndpoints;

namespace WebApi2._0.Features.Products.MasterItems;

public sealed class MasterItems : Group
{
   public MasterItems()
   {
      Configure("master-items", ep =>
      {
         ep.Description(x => x.Produces(200).WithTags("Master Items"));
      });
   }
}