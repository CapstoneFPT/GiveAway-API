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
using BusinessObjects.Entities;
using BusinessObjects.Utils;
using Microsoft.EntityFrameworkCore;

namespace Services.Accounts
{
    public class AccountService : IAccountService
    {
        private readonly IAccountRepository _account;
        private readonly IMapper _mapper;

        public AccountService(IAccountRepository repository, IMapper mapper)
        {
            _account = repository;
            _mapper = mapper;
        }

        public async Task<Result<AccountResponse>> BanAccountById(Guid id)
        {
            var user = await _account.GetAccountById(id);
            var response = new Result<AccountResponse>();
            if (user == null)
            {
                response.Messages = ["User does not existed"];
                response.ResultStatus = ResultStatus.NotFound;
                return response;
            }
            else if (user.Status.Equals(AccountStatus.Inactive))
            {
                response.Messages = ["This account is already inactive"];
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

        public async Task<Result<AccountResponse>> GetAccountById(Guid id)
        {
            var response = new Result<AccountResponse>();
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

        public async Task<Result<AccountResponse>> UpdateAccount(Guid id, UpdateAccountRequest request)
        {
            var response = new Result<AccountResponse>();
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

        public async Task DeductPoints(Guid requestMemberId, int orderTotalPrice)
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
           
            Expression<Func<Account, bool>> predicate = account => request.Status.Length != 0 ?
                EF.Functions.ILike(account.Fullname, request.SearchTerm) && request.Status.Contains( account.Status) : EF.Functions.ILike(account.Fullname, request.SearchTerm);
            Expression<Func<Account, AccountResponse>> selector = account => new AccountResponse()
            {
                Fullname = account.Fullname,
                AccountId = account.AccountId,
                Phone = account.Phone,
                Email = account.Email,
                Status = account.Status,
                Role = account.Role
            };
            (List<AccountResponse> Items, int Page, int PageSize, int TotalCount) data = await _account.GetAccounts<AccountResponse>(
                request.Page, request.PageSize, predicate, selector,isTracking:false
            );

            return new PaginationResponse<AccountResponse>()
            {
                Items = data.Items,
                PageSize = data.PageSize,
                PageNumber = data.Page,
                Filters = [$"Account Status : {request.Status}"]
            };
        }
    }
}