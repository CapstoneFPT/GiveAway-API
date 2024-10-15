using System.ComponentModel.DataAnnotations;
using FastEndpoints;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using WebApi2._0.Common;
using WebApi2._0.Domain.Entities;
using WebApi2._0.Domain.Enums;
using WebApi2._0.Domain.ValueObjects;
using WebApi2._0.Infrastructure.Persistence;

namespace WebApi2._0.Features.Admin.Accounts.GetAccounts;

public sealed class GetAccountsEndpoint : Endpoint<GetAccountsRequest, PaginationResponse<AccountsListResponse>>
{
    private readonly GiveAwayDbContext _dbContext;

    public GetAccountsEndpoint(GiveAwayDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public override void Configure()
    {
        Get("accounts");
        Roles("Admin");
    }

    public override async Task<PaginationResponse<AccountsListResponse>> ExecuteAsync(GetAccountsRequest req,
        CancellationToken ct)
    {
        var query = _dbContext.Accounts.AsQueryable();

        query = query.Where(GetAccountsPredicate.GetPredicate(req));
        var count = await query.CountAsync(ct);

        var data = await query
            .OrderBy(x => x.Fullname)
            .Skip(PaginationUtils.GetSkip(req.Page, req.PageSize))
            .Take(PaginationUtils.GetTake(req.PageSize))
            .Select(x => new AccountsListResponse
            {
                AccountId = x.AccountId,
                Email = x.Email,
                Phone = x.Phone,
                Fullname = x.Fullname,
                Role = x.Role,
                Balance = x.Balance,
                Status = x.Status,
                ShopId = (x as Staff).Shop.ShopId,
                ShopCode = (x as Staff).Shop.ShopCode
            })
            .ToListAsync(ct);

        return new PaginationResponse<AccountsListResponse>()
        {
            Items = data,
            PageNumber = req.Page ?? 1,
            PageSize = req.PageSize ?? int.MaxValue,
            TotalCount = count
        };
    }
}

public record GetAccountsRequest
{
    public int? Page { get; set; }
    public int? PageSize { get; set; }
    public string? Fullname { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public Roles[]? Roles { get; set; }
    public AccountStatus[]? Status { get; set; } = [];
}

public record AccountsListResponse
{
    public Guid AccountId { get; set; }
    [EmailAddress] public string Email { get; set; }
    public string Phone { get; set; }
    public string Fullname { get; set; }
    public Roles Role { get; set; }
    public decimal Balance { get; set; }
    public AccountStatus Status { get; set; }
    public Guid? ShopId { get; set; }
    public string? ShopCode { get; set; }
}