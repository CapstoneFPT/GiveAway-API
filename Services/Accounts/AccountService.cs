using AutoMapper;
using BusinessObjects.Dtos.Account.Request;
using BusinessObjects.Dtos.Account.Response;
using BusinessObjects.Dtos.Auth;
using BusinessObjects.Dtos.Commons;
using Repositories.Accounts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using BusinessObjects.Dtos.Account;
using BusinessObjects.Dtos.Inquiries;
using BusinessObjects.Dtos.Transactions;
using BusinessObjects.Dtos.Withdraws;
using BusinessObjects.Entities;
using BusinessObjects.Utils;
using DotNext;
using LinqKit;
using Microsoft.EntityFrameworkCore;
using Repositories.BankAccounts;
using Repositories.Inquiries;
using Repositories.Transactions;
using Repositories.Withdraws;

namespace Services.Accounts
{
    public class AccountService : IAccountService
    {
        private readonly IAccountRepository _account;
        private readonly IInquiryRepository _inquiryRepository;
        private readonly IWithdrawRepository _withdrawRepository;
        private readonly ITransactionRepository _transactionRepository;
        private readonly IBankAccountRepository _bankAccountRepository;
        private readonly IMapper _mapper;

        public AccountService(IAccountRepository repository, IMapper mapper, IInquiryRepository inquiryRepository,
            IWithdrawRepository withdrawRepository, ITransactionRepository transactionRepository)
        {
            _account = repository;
            _mapper = mapper;
            _inquiryRepository = inquiryRepository;
            _withdrawRepository = withdrawRepository;
            _transactionRepository = transactionRepository;
        }

        public async Task<BusinessObjects.Dtos.Commons.Result<AccountResponse>> BanAccountById(Guid id)
        {
            var user = await _account.GetAccountById(id);
            var response = new BusinessObjects.Dtos.Commons.Result<AccountResponse>();
            if (user == null)
            {
                response.Messages = ["User does not existed"];
                response.ResultStatus = ResultStatus.NotFound;
                return response;
            }
            else if (user.Status.Equals(AccountStatus.Inactive))
            {
                user.Status = AccountStatus.Active;
                await _account.UpdateAccount(user);
                response.Data = _mapper.Map<AccountResponse>(user);
                response.Messages = ["This account has been changed to active"];
                response.ResultStatus = ResultStatus.Error;
                return response;
            }
            else
            {
                user.Status = AccountStatus.Inactive;
                await _account.UpdateAccount(user);
                response.Data = _mapper.Map<AccountResponse>(user);
                response.Messages = ["This account has been changed to inactive"];
                response.ResultStatus = ResultStatus.Success;
                return response;
            }
        }

        public async Task<BusinessObjects.Dtos.Commons.Result<AccountResponse>> GetAccountById(Guid id)
        {
            var response = new BusinessObjects.Dtos.Commons.Result<AccountResponse>();
            var user = await _account.GetAccountById(id);
            if (user == null)
            {
                response.Messages = ["User not found!"];
                response.ResultStatus = ResultStatus.NotFound;
                return response;
            }
            else
            {
                response.Data = _mapper.Map<AccountResponse>(user);
                response.Messages = ["Successfully!"];
                response.ResultStatus = ResultStatus.Success;
                return response;
            }
        }

        public async Task<List<AccountResponse>> GetAllAccounts()
        {
            var list = await _account.GetAllAccounts();
            return _mapper.Map<List<AccountResponse>>(list);
        }

        public async Task<BusinessObjects.Dtos.Commons.Result<AccountResponse>> UpdateAccount(Guid id,
            UpdateAccountRequest request)
        {
            var response = new BusinessObjects.Dtos.Commons.Result<AccountResponse>();
            var user = await _account.GetAccountById(id);
            if (user == null)
            {
                response.Messages = ["User is not found!"];
                response.ResultStatus = ResultStatus.NotFound;
                return response;
            }
            else if (request.Phone.Equals(user.Phone) && request.Fullname.Equals(user.Fullname))
            {
                response.Data = _mapper.Map<AccountResponse>(user);
                response.Messages = ["Nothing change!"];
                response.ResultStatus = ResultStatus.Error;
                return response;
            }

            var newuser = _mapper.Map(request, user);
            response.Data = _mapper.Map<AccountResponse>(await _account.UpdateAccount(newuser));
            response.Messages = ["Update successfully"];
            response.ResultStatus = ResultStatus.Success;
            return response;
        }

        public async Task DeductPoints(Guid requestMemberId, decimal orderTotalPrice)
        {
            var member = await _account.GetAccountById(requestMemberId);

            if (member.Balance < orderTotalPrice)
            {
                throw new InsufficientBalanceException();
            }

            member.Balance -= orderTotalPrice;
            await _account.UpdateAccount(member);
        }

        public async Task<PaginationResponse<AccountResponse>> GetAccounts(GetAccountsRequest request)
        {
            Expression<Func<Account, bool>> predicate = account => true;

            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                predicate = account => EF.Functions.ILike(account.Fullname, $"%{
                    request.SearchTerm
                }%");
            }

            if (!string.IsNullOrWhiteSpace(request.Phone))
            {
                predicate = account => EF.Functions.ILike(account.Phone, $"%{request.Phone}%");
            }

            if (request.Status != null && request.Status.Length != 0)
            {
                predicate = predicate.And(account => request.Status.Contains(account.Status));
            }

            if (request.Role != null)
            {
                predicate = predicate.And(account => account.Role == request.Role);
            }


            Expression<Func<Account, AccountResponse>> selector = account => new AccountResponse()
            {
                Fullname = account.Fullname,
                AccountId = account.AccountId,
                Phone = account.Phone,
                Email = account.Email,
                Status = account.Status,
                Role = account.Role
            };
            (List<AccountResponse> Items, int Page, int PageSize, int TotalCount) data =
                await _account.GetAccounts<AccountResponse>(
                    request.Page, request.PageSize, predicate, selector, isTracking: false
                );

            return new PaginationResponse<AccountResponse>()
            {
                Items = data.Items,
                PageSize = data.PageSize,
                PageNumber = data.Page,
                Filters = [$"Account Status : {request.Status}"],
                TotalCount = data.TotalCount,
                SearchTerm = request.SearchTerm
            };
        }

        public async Task<CreateInquiryResponse> CreateInquiry(Guid accountId, CreateInquiryRequest request)
        {
            var inquiry = new Inquiry()
            {
                Message = request.Message,
                MemberId = accountId,
                Status = InquiryStatus.Processing,
                CreatedDate = DateTime.UtcNow
            };

            var result = await _inquiryRepository.CreateInquiry(inquiry);
            var account = await _account.GetAccountById(accountId);
            return new CreateInquiryResponse()
            {
                InquiryId = result.InquiryId,
                MemberId = result.MemberId,

                Fullname = account.Fullname,
                Email = account.Email,
                Phone = account.Phone,
                Message = result.Message,
                CreatedDate = result.CreatedDate,
                InquiryStatus = result.Status
            };
        }

        public async Task<CreateWithdrawResponse> RequestWithdraw(Guid accountId, CreateWithdrawRequest request)
        {
            var account = await _account.GetMemberById(accountId);

            if (account == null)
            {
                throw new AccountNotFoundException();
            }

            if (account.Balance < request.Amount)
            {
                throw new InsufficientBalanceException();
            }

            if (request.Bank == null || request.BankAccountName == null || request.BankAccountNumber == null)
            {
                throw new BankAccountNotSetException("Please set your bank account first");
            }


            var newWithdraw = new Withdraw()
            {
                Amount = request.Amount,
                MemberId = accountId,
                CreatedDate = DateTime.UtcNow,
                Status = WithdrawStatus.Processing,
                Bank = request.Bank,
                BankAccountName = request.BankAccountName,
                BankAccountNumber = request.BankAccountNumber
            };

            var result = await _withdrawRepository.CreateWithdraw(newWithdraw);

            account.Balance -= request.Amount;
            await _account.UpdateAccount(account);

            var transaction = new Transaction
            {
                Amount = result.Amount,
                CreatedDate = DateTime.UtcNow,
                MemberId = result.MemberId,
                Type = TransactionType.Withdraw,
            };

            await _transactionRepository.CreateTransaction(transaction);
            return new CreateWithdrawResponse()
            {
                WithdrawId = result.WithdrawId,
                Amount = result.Amount,
                AmountLeft = account.Balance,
                Bank = result.Bank,
                BankAccountName = result.BankAccountName,
                BankAccountNumber = result.BankAccountNumber,
                MemberId = result.MemberId,
                Status = result.Status,
                CreatedDate = result.CreatedDate,
            };
        }

        public async Task<PaginationResponse<GetTransactionsResponse>> GetTransactions(Guid accountId,
            GetTransactionsRequest request)
        {
            Expression<Func<Transaction, bool>> predicate = transaction => transaction.MemberId == accountId;

            if (request.Types.Length != 0)
            {
                predicate = predicate.And(x => request.Types.Contains(x.Type));
            }

            Expression<Func<Transaction, DateTime>> orderBy = transaction => transaction.CreatedDate;
            Expression<Func<Transaction, GetTransactionsResponse>> selector = transaction =>
                new GetTransactionsResponse()
                {
                    TransactionId = transaction.TransactionId,
                    MemberId = transaction.MemberId,
                    Amount = transaction.Amount,
                    Type = transaction.Type,
                    CreatedDate = transaction.CreatedDate,
                    OrderCode = transaction.Order != null ? transaction.Order.OrderCode : null,
                    ConsignSaleCode = transaction.ConsignSale != null ? transaction.ConsignSale.ConsignSaleCode : null
                };

            (List<GetTransactionsResponse> Items, int Page, int PageSize, int TotalCount) data = await
                _transactionRepository.GetTransactionsProjection<GetTransactionsResponse>(request.Page,
                    request.PageSize,
                    predicate, orderBy, selector);

            return new PaginationResponse<GetTransactionsResponse>()
            {
                Items = data.Items,
                PageSize = data.PageSize,
                PageNumber = data.Page,
                TotalCount = data.TotalCount
            };
        }

        public async Task<PaginationResponse<GetWithdrawsResponse>> GetWithdraws(Guid accountId,
            GetWithdrawsRequest request)
        {
            Expression<Func<Withdraw, bool>> predicate = withdraw => withdraw.MemberId == accountId;

            if (request.Status != null)
            {
                predicate = predicate.And(x => x.Status == request.Status);
            }


            Expression<Func<Withdraw, GetWithdrawsResponse>> selector = withdraw => new GetWithdrawsResponse()
            {
                WithdrawId = withdraw.WithdrawId,
                MemberId = withdraw.MemberId,
                Amount = withdraw.Amount,
                Status = withdraw.Status,
                Bank = withdraw.Bank,
                BankAccountName = withdraw.BankAccountName,
                BankAccountNumber = withdraw.BankAccountNumber,
                CreatedDate = withdraw.CreatedDate
            };

            (List<GetWithdrawsResponse> Items, int Page, int PageSize, int TotalCount) data = await
                _withdrawRepository.GetWithdraws(request.Page, request.PageSize,
                    predicate, selector, isTracking: false);

            return new PaginationResponse<GetWithdrawsResponse>()
            {
                Items = data.Items,
                PageSize = data.PageSize,
                PageNumber = data.Page,
                TotalCount = data.TotalCount
            };
        }

        public async Task<Result<List<BankAccountsListResponse>, ErrorCode>> GetBankAccounts(Guid accountId)
        {
            Expression<Func<BankAccount, bool>> predicate = bankAccount => bankAccount.MemberId == accountId;
            Expression<Func<BankAccount, BankAccountsListResponse>> selector = bankAccount =>
                new BankAccountsListResponse()
                {
                    BankAccountId = bankAccount.BankAccountId,
                    BankAccountName = bankAccount.BankAccountName ?? "N/A",
                    BankAccountNumber = bankAccount.BankAccountNumber ?? "N/A", BankName = bankAccount.Bank ?? "N/A",
                    IsDefault = bankAccount.IsDefault
                };

            try
            {
                var result = await _bankAccountRepository.GetQueryable()
                    .Where(predicate)
                    .Select(selector)
                    .ToListAsync();

                return new Result<List<BankAccountsListResponse>, ErrorCode>(result);
            }
            catch (Exception e)
            {
                return new Result<List<BankAccountsListResponse>, ErrorCode>(ErrorCode.ServerError);
            }
        }
    }
}