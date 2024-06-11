using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.Dtos.Account.Request
{
    public class UpdateAccountRequest
    {
        [Required, Phone]
        public string Phone { get; set; }
        [Required]
        public string Fullname { get; set; }
    }
}
