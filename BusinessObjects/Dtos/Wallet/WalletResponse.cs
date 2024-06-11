using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.Dtos.Wallet
{
    public class WalletResponse
    {
        public int Balance { get; set; }
        public string? BankAccountNumber { get; set; }
        public string? BankName { get; set; }
        public string AccountName { get; set; }
    }
}
