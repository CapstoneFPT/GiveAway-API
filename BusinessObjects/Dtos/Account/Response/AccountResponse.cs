using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.Dtos.Account.Response
{
    public class AccountResponse
    {
        public Guid AccountId { get; set; }
        [EmailAddress] public string Email { get; set; }
        public string Phone { get; set; }
        public string Fullname { get; set; }
        public string Role { get; set; }
        public string Status { get; set; }
    }
}
