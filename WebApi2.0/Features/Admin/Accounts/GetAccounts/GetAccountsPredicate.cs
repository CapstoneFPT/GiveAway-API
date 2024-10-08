using System.Linq.Expressions;
using LinqKit;
using Microsoft.EntityFrameworkCore;
using WebApi2._0.Domain.Entities;

namespace WebApi2._0.Features.Admin.Accounts.GetAccounts;

public static class GetAccountsPredicate
{
    public static Expression<Func<Account, bool>> GetPredicate(GetAccountsRequest req)
    {
        var predicates = new Dictionary<
            Func<GetAccountsRequest, bool>,
            Func<GetAccountsRequest, Expression<Func<Account, bool>>>
        >()
        {
            [x=>x.Fullname != null] = GetAccountFullnamePredicate,
            [x=>x.Roles is {Length: > 0}] = GetAccountRolePredicate,
            [x=>x.Status is {Length: > 0}] = GetAccountStatusPredicate,
            [x=>x.Email != null] = GetAccountEmailPredicate,
            [x=>x.Phone != null] = GetAccountPhonePredicate
        };

        return predicates
            .Where(x => x.Key(req) == true)
            .Select(x => x.Value(req))
            .Aggregate(PredicateBuilder.New<Account>(true), (current, next) => current.And(next));
    }

    private static Expression<Func<Account, bool>> GetAccountPhonePredicate(GetAccountsRequest arg)
    {
        return x => EF.Functions.ILike(x.Phone, $"%{arg.Phone}%");
    }

    private static Expression<Func<Account, bool>> GetAccountEmailPredicate(GetAccountsRequest arg)
    {
        return x => EF.Functions.ILike(x.Email, $"%{arg.Email}%");
    }

    private static Expression<Func<Account, bool>> GetAccountStatusPredicate(GetAccountsRequest arg)
    {
        return x=> arg.Status != null && arg.Status.Contains(x.Status);
    }

    private static Expression<Func<Account, bool>> GetAccountRolePredicate(GetAccountsRequest arg)
    {
        return x => arg.Roles != null && arg.Roles.Contains(x.Role);
    }

    private static Expression<Func<Account, bool>> GetAccountFullnamePredicate(GetAccountsRequest arg)
    {
        return x=> EF.Functions.ILike(x.Fullname, $"%{arg.Fullname}%");
    }
}