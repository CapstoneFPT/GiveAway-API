using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.Dtos.Refunds
{
    public class CreateRefundRequest{
        [Required]
        public Guid OrderDetailIds { get; set; }
        public required string Description { get; set; }
        public required string[] Images { get; set; }
    }
}