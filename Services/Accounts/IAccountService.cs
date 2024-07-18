using BusinessObjects.Dtos.Account.Request;
using BusinessObjects.Dtos.Account.Response;
using BusinessObjects.Dtos.Commons;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessObjects.Dtos.Account;
using BusinessObjects.Dtos.Inquiries;
using BusinessObjects.Dtos.Withdraws;

namespace Services.Accounts
{
    public interface IAccountService
    {
        Task<List<AccountResponse>> GetAllAccounts();
        Task<Result<AccountResponse>> GetAccountById(Guid id);
        Task<Result<AccountResponse>> BanAccountById(Guid id);
        Task<Result<AccountResponse>> UpdateAccount(Guid id, UpdateAccountRequest request);
        Task DeductPoints(Guid requestMemberId, int orderTotalPrice);
        Task<PaginationResponse<AccountResponse>> GetAccounts(GetAccountsRequest request);
        Task<CreateInquiryResponse> CreateInquiry(Guid accountId, CreateInquiryRequest request);
        Task<object?> RequestWithdraw(Guid accountId, CreateWithdrawRequest request);
    }
}
