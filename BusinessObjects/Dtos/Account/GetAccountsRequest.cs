﻿using BusinessObjects.Dtos.Commons;

namespace BusinessObjects.Dtos.Account;

public class GetAccountsRequest
{
    public int Page { get; set; }
    public int PageSize { get; set; }
    public string SearchTerm { get; set; }
    public AccountStatus[] Status { get; set; } = [];
}