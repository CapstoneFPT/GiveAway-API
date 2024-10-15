using FastEndpoints;

namespace WebApi2._0.Features.Products.FashionItems;

public sealed class FashionItems : Group
{
   public FashionItems()
   {
      Configure("fashion-items", ep =>
      {
         ep.Description(x => x.WithTags("FashionItems"));
      });
   }
}