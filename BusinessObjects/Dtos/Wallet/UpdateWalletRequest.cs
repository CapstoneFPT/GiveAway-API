using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.Dtos.Wallet
{
    public class UpdateWalletRequest
    {
        [Required]
        public string BankAccountNumber { get; set; }
        [Required]
        public string BankName { get; set; }
    }
}
