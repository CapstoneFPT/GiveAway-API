using BusinessObjects.Dtos.Account.Request;
using BusinessObjects.Dtos.Account.Response;
using BusinessObjects.Dtos.Commons;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessObjects.Dtos.Account;
using BusinessObjects.Dtos.AuctionDeposits;
using BusinessObjects.Dtos.Auctions;
using BusinessObjects.Dtos.Inquiries;
using BusinessObjects.Dtos.Transactions;
using BusinessObjects.Dtos.Withdraws;
using DotNext;

namespace Services.Accounts
{
    public interface IAccountService
    {
        Task<List<AccountResponse>> GetAllAccounts();
        Task<BusinessObjects.Dtos.Commons.Result<AccountResponse>> GetAccountById(Guid id);
        Task<BusinessObjects.Dtos.Commons.Result<AccountResponse>> BanAccountById(Guid id);
        Task<BusinessObjects.Dtos.Commons.Result<AccountResponse>> UpdateAccount(Guid id, UpdateAccountRequest request);
        Task DeductPoints(Guid requestMemberId, decimal orderTotalPrice);
        Task<PaginationResponse<AccountResponse>> GetAccounts(GetAccountsRequest request);
        Task<CreateInquiryResponse> CreateInquiry(Guid accountId, CreateInquiryRequest request);
        Task<CreateWithdrawResponse> RequestWithdraw(Guid accountId, CreateWithdrawRequest request);
        Task<PaginationResponse<AccountTransactionsListResponse>> GetTransactions(Guid accountId, GetTransactionsRequest request);
        Task<PaginationResponse<GetWithdrawsResponse>> GetWithdraws(Guid accountId, GetWithdrawsRequest request);
        Task<Result<List<BankAccountsListResponse>, ErrorCode>> GetBankAccounts(Guid accountId);
        Task<Result<CreateBankAccountResponse,ErrorCode>> CreateBankAccount(Guid accountId, CreateBankAccountRequest request);
        Task<Result<UpdateBankAccountResponse,ErrorCode>> UpdateBankAccount(Guid accountId, Guid bankAccountId, UpdateBankAccountRequest request);
        Task<Result<DeleteBankAccountResponse,ErrorCode>> DeleteBankAccount(Guid accountId, Guid bankAccountId);
        Task<Result<UpdateBankAccountResponse, ErrorCode>> SetDefaultBankAccount(Guid accountId, Guid bankAccountId);
        Task<DotNext.Result<PaginationResponse<AuctionListResponse>>> GetAuctions(Guid accountId, GetAccountAuctionsRequest request);
        Task<Result<PaginationResponse<AccountDepositsListResponse>,ErrorCode>> GetDeposits(Guid accountId, GetDepositsRequest request);
    }
}
