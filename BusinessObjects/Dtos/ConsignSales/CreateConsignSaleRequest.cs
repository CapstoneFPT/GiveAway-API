using BusinessObjects.Dtos.Commons;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.Dtos.ConsignSales
{
    public class CreateConsignSaleRequest
    {
        public ConsignSaleType Type { get; set; }
        public Guid ShopId { get; set; }
        public List<AddFashionItemForConsignRequest> fashionItemForConsigns { get; set; } = new List<AddFashionItemForConsignRequest>();
    }
}
