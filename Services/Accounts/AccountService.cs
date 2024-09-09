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
using System.Net;
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
using Microsoft.AspNetCore.Mvc;
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
            IWithdrawRepository withdrawRepository, ITransactionRepository transactionRepository,
            IBankAccountRepository bankAccountRepository)
        {
            _account = repository;
            _mapper = mapper;
            _inquiryRepository = inquiryRepository;
            _withdrawRepository = withdrawRepository;
            _transactionRepository = transactionRepository;
            _bankAccountRepository = bankAccountRepository;
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

        private Expression<Func<Transaction, bool>> GetPredicate(GetTransactionsRequest request)
        {
            Expression<Func<Transaction, bool>> predicate = transaction => true;

            if (request.Types.Length != 0)
            {
                predicate = predicate.And(x => request.Types.Contains(x.Type));
            }

            if (!string.IsNullOrWhiteSpace(request.OrderCode))
            {
                predicate = predicate.And(x =>
                    x.Order != null && EF.Functions.ILike(x.Order.OrderCode, $"%{request.OrderCode}%"));
            }

            if (!string.IsNullOrWhiteSpace(request.ConsignSaleCode))
            {
                predicate = predicate.And(x =>
                    x.ConsignSale != null &&
                    EF.Functions.ILike(x.ConsignSale.ConsignSaleCode, $"%{request.ConsignSaleCode}%"));
            }

            if (!string.IsNullOrWhiteSpace(request.RechargeCode))
            {
                predicate = predicate.And(x =>
                    x.Recharge != null && EF.Functions.ILike(x.Recharge.RechargeCode, $"%{request.RechargeCode}%"));
            }

            if (!string.IsNullOrWhiteSpace(request.DepositCode))
            {
                predicate = predicate.And(x =>
                    x.AuctionDeposit != null &&
                    EF.Functions.ILike(x.AuctionDeposit.DepositCode, $"%{request.DepositCode}%"));
            }

            if (!string.IsNullOrWhiteSpace(request.TransactionCode))
            {
                predicate = predicate.And(x => EF.Functions.ILike(x.TransactionCode, $"%{request.TransactionCode}%"));
            }

            return predicate;
        }

        public async Task<PaginationResponse<GetTransactionsResponse>> GetTransactions(Guid accountId,
            GetTransactionsRequest request)
        {
            Expression<Func<Transaction, bool>> predicate = GetPredicate(request);
            predicate = predicate.And(x=>x.MemberId == accountId);
            
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
                    ConsignSaleCode = transaction.ConsignSale != null
                        ? transaction.ConsignSale.ConsignSaleCode
                        : null,
                    RechargeCode = transaction.Recharge != null ? transaction.Recharge.RechargeCode : null,
                    DepositCode =
                        transaction.AuctionDeposit != null ? transaction.AuctionDeposit.DepositCode : null,
                    TransactionCode = transaction.TransactionCode
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


            if (!string.IsNullOrWhiteSpace(request.WithdrawCode))
            {
                predicate = predicate.And(x => EF.Functions.ILike(x.WithdrawCode, $"%{request.WithdrawCode}%"));
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
                WithdrawCode = withdraw.WithdrawCode,
                CreatedDate = withdraw.CreatedDate
            };

            Expression<Func<Withdraw, DateTime>> orderBy = withdraw => withdraw.CreatedDate;

            (List<GetWithdrawsResponse> Items, int Page, int PageSize, int TotalCount) data = await
                _withdrawRepository.GetWithdraws(request.Page, request.PageSize,
                    predicate, selector, isTracking: false, orderBy: orderBy);

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
                    BankAccountNumber = bankAccount.BankAccountNumber ?? "N/A",
                    BankName = bankAccount.Bank ?? "N/A",
                    BankLogo = bankAccount.BankLogo ?? "N/A",
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

        public async Task<Result<CreateBankAccountResponse, ErrorCode>> CreateBankAccount(Guid accountId,
            CreateBankAccountRequest request)
        {
            // if (await CheckBankAccountExisted(request.BankName, request.BankAccountName, request.BankAccountNumber))
            // {
            //     return new Result<CreateBankAccountResponse, ErrorCode>(ErrorCode.DuplicateBankAccount);
            // }

            var bankAccount = new BankAccount
            {
                Bank = request.BankName,
                BankAccountName = request.BankAccountName,
                BankAccountNumber = request.BankAccountNumber,
                BankLogo = request.BankLogo,
                MemberId = accountId,
                IsDefault = !await _bankAccountRepository
                    .GetQueryable()
                    .Where(x => x.MemberId == accountId).AnyAsync(),
                CreatedDate = DateTime.UtcNow
            };

            try
            {
                var result = await _bankAccountRepository.CreateBankAccount(bankAccount);

                return new Result<CreateBankAccountResponse, ErrorCode>(new CreateBankAccountResponse
                {
                    MemberId = accountId,
                    BankAccountId = result.BankAccountId,
                    BankAccountName = result.BankAccountName,
                    BankAccountNumber = result.BankAccountNumber,
                    BankName = result.Bank,
                });
            }
            catch (Exception e)
            {
                return new Result<CreateBankAccountResponse, ErrorCode>(ErrorCode.ServerError);
            }
        }

        public async Task<Result<UpdateBankAccountResponse, ErrorCode>> UpdateBankAccount(Guid accountId,
            Guid bankAccountId, UpdateBankAccountRequest request)
        {
            var existedBankAccount = await _bankAccountRepository.GetQueryable()
                .FirstOrDefaultAsync(x => x.BankAccountId == bankAccountId);

            if (existedBankAccount == null)
            {
                return new Result<UpdateBankAccountResponse, ErrorCode>(ErrorCode.NotFound);
            }

            if (existedBankAccount.MemberId != accountId)
            {
                return new Result<UpdateBankAccountResponse, ErrorCode>(ErrorCode.Unauthorized);
            }

            existedBankAccount.Bank = request.BankName ?? existedBankAccount.Bank;
            existedBankAccount.BankAccountName = request.BankAccountName ?? existedBankAccount.BankAccountName;
            existedBankAccount.BankAccountNumber =
                request.BankAccountNumber ?? existedBankAccount.BankAccountNumber;
            existedBankAccount.BankLogo = request.BankLogo ?? existedBankAccount.BankLogo;
            existedBankAccount.IsDefault = request.IsDefault ?? existedBankAccount.IsDefault;


            try
            {
                var otherBankAccounts = await _bankAccountRepository
                    .GetQueryable()
                    .Where(x =>
                        x.MemberId == accountId
                        && x.BankAccountId != bankAccountId)
                    .ToListAsync();

                if (existedBankAccount.IsDefault)
                {
                    foreach (var otherBankAccount in otherBankAccounts)
                    {
                        otherBankAccount.IsDefault = false;
                    }

                    await _bankAccountRepository.UpdateRange(otherBankAccounts);
                }
                else
                {
                    if (otherBankAccounts.Count == 0)
                        return new Result<UpdateBankAccountResponse, ErrorCode>(ErrorCode.NoBankAccountLeft);
                }

                await _bankAccountRepository.UpdateBankAccount(existedBankAccount);
                return new Result<UpdateBankAccountResponse, ErrorCode>(new UpdateBankAccountResponse
                {
                    BankAccountId = existedBankAccount.BankAccountId,
                    BankName = existedBankAccount.Bank ?? "N/A",
                    BankAccountName = existedBankAccount.BankAccountName ?? "N/A",
                    BankAccountNumber = existedBankAccount.BankAccountNumber ?? "N/A",
                    MemberId = accountId
                });
            }
            catch (Exception e)
            {
                return new Result<UpdateBankAccountResponse, ErrorCode>(ErrorCode.ServerError);
            }
        }

        public async Task<Result<DeleteBankAccountResponse, ErrorCode>> DeleteBankAccount(Guid accountId,
            Guid bankAccountId)
        {
            var existedBankAccount = await _bankAccountRepository
                .GetQueryable()
                .FirstOrDefaultAsync(x => x.BankAccountId == bankAccountId);

            if (existedBankAccount == null)
            {
                return new Result<DeleteBankAccountResponse, ErrorCode>(ErrorCode.NotFound);
            }

            if (existedBankAccount.MemberId != accountId)
            {
                return new Result<DeleteBankAccountResponse, ErrorCode>(ErrorCode.Unauthorized);
            }

            try
            {
                if (existedBankAccount.IsDefault)
                {
                    var prevBankAccount = await _bankAccountRepository
                        .GetQueryable()
                        .Where(x =>
                            x.CreatedDate < existedBankAccount.CreatedDate && x.MemberId == accountId)
                        .OrderByDescending(x => x.CreatedDate)
                        .FirstOrDefaultAsync();
                    if (prevBankAccount != null)
                    {
                        prevBankAccount.IsDefault = true;
                        await _bankAccountRepository.UpdateBankAccount(prevBankAccount);
                    }
                    else
                    {
                        return new Result<DeleteBankAccountResponse, ErrorCode>(ErrorCode.NoBankAccountLeft);
                    }
                }

                await _bankAccountRepository.DeleteBankAccount(existedBankAccount);
                return new Result<DeleteBankAccountResponse, ErrorCode>(new DeleteBankAccountResponse
                {
                    BankAccountId = existedBankAccount.BankAccountId,
                    BankName = existedBankAccount.Bank ?? "N/A",
                    BankAccountName = existedBankAccount.BankAccountName ?? "N/A",
                    BankAccountNumber = existedBankAccount.BankAccountNumber ?? "N/A",
                    IsDefault = existedBankAccount.IsDefault,
                    MemberId = accountId
                });
            }
            catch (Exception e)
            {
                return new Result<DeleteBankAccountResponse, ErrorCode>(ErrorCode.ServerError);
            }
        }

        public async Task<Result<UpdateBankAccountResponse, ErrorCode>> SetDefaultBankAccount(Guid accountId,
            Guid bankAccountId)
        {
            var existedBankAccount = await _bankAccountRepository
                .GetQueryable()
                .FirstOrDefaultAsync(x => x.BankAccountId == bankAccountId);

            if (existedBankAccount == null)
            {
                return new Result<UpdateBankAccountResponse, ErrorCode>(ErrorCode.NotFound);
            }

            if (existedBankAccount.MemberId != accountId)
            {
                return new Result<UpdateBankAccountResponse, ErrorCode>(ErrorCode.Unauthorized);
            }

            try
            {
                var otherBankAccounts = await _bankAccountRepository
                    .GetQueryable()
                    .Where(x =>
                        x.BankAccountId != bankAccountId
                        && x.IsDefault)
                    .ToListAsync();

                foreach (var otherBankAccount in otherBankAccounts)
                {
                    otherBankAccount.IsDefault = false;
                }

                await _bankAccountRepository.UpdateRange(otherBankAccounts);
                existedBankAccount.IsDefault = true;
                await _bankAccountRepository.UpdateBankAccount(existedBankAccount);
                return new Result<UpdateBankAccountResponse, ErrorCode>(new UpdateBankAccountResponse
                {
                    BankAccountId = existedBankAccount.BankAccountId,
                    BankName = existedBankAccount.Bank ?? "N/A",
                    BankAccountName = existedBankAccount.BankAccountName ?? "N/A",
                    BankAccountNumber = existedBankAccount.BankAccountNumber ?? "N/A",
                    IsDefault = existedBankAccount.IsDefault,
                    MemberId = accountId
                });
            }
            catch (Exception e)
            {
                return new Result<UpdateBankAccountResponse, ErrorCode>(ErrorCode.ServerError);
            }
        }

        private Task<bool> CheckBankAccountExisted(string bank, string accountName, string accountNumber)
        {
            Expression<Func<BankAccount, bool>> predicate = account =>
                account.Bank == bank && account.BankAccountName == accountName &&
                account.BankAccountNumber == accountNumber;

            return _bankAccountRepository
                .GetQueryable()
                .AnyAsync(predicate);
        }
    }
}