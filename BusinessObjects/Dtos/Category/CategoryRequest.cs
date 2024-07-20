using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessObjects.Dtos.Commons;

namespace BusinessObjects.Dtos.Category
{
    public class CategoryRequest
    {
        public Guid? CategoryId { get; set; }
        public string? SearchName { get; set; }
        public Guid? ParentId { get; set; }
        public int? Level { get; set; }
        public CategoryStatus? Status { get; set; }
    }
}
