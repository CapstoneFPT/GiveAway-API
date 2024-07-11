﻿using BusinessObjects.Dtos.Commons;
using BusinessObjects.Dtos.ConsignSales;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Services.Revenue;

namespace WebApi.Controllers;

[ApiController]
[Route("api/dashboard")]
public class DashboardController : ControllerBase

{
    private readonly IRevenueService _revenueService;

    public DashboardController(IRevenueService revenueService)
    {
        _revenueService = revenueService;
    }

    [HttpGet("shop/{shopId}/revenue")]
    public async Task<ActionResult<ShopRevenueDto>> GetShopRevenue(
        Guid shopId,
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate)
    {
        try
        {
            var revenue = await _revenueService.GetShopRevenue(shopId, startDate, endDate);
            return Ok(revenue);
        }
        catch (Exception ex)
        {
            return BadRequest(new Result<ShopRevenueDto>
            {
                ResultStatus = ResultStatus.Error,
                Messages = new[] { $"Error retrieving shop revenue: {ex.Message}" }
            });
        }
    }

    [HttpGet("system/revenue")]
    public async Task<ActionResult<SystemRevenueDto>> GetSystemRevenue(
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate)
    {
        try
        {
            var revenue = await _revenueService.GetSystemRevenue(startDate, endDate);
            return Ok(revenue);
        }
        catch (Exception ex)
        {
            return BadRequest(new Result<SystemRevenueDto>
            {
                ResultStatus = ResultStatus.Error,
                Messages = new[] { $"Error retrieving system revenue: {ex.Message}" }
            });
        }
    }
    
    [HttpGet("/monthly-revenue")]
    public async Task<ActionResult<MonthlyRevenueDto>> GetMonthlyRevenue(
        [FromQuery] int year,
        [FromQuery] Guid? shopId)
    {
        try
        {
            var revenue = await _revenueService.GetMonthlyRevenue(year, shopId);
            return Ok(revenue);
        }
        catch (Exception ex)
        {
            return BadRequest(new Result<MonthlyRevenueDto>
            {
                ResultStatus = ResultStatus.Error,
                Messages = new[] { $"Error retrieving monthly revenue: {ex.Message}" }
            });
        }
    }
    
    [HttpGet("/monthly-payouts")]
    public async Task<ActionResult<MonthlyPayoutsResponse>> GetMonthlyPayouts(
        [FromQuery] int year,
        [FromQuery] Guid? shopId)
    {
        try
        {
            var revenue = await _revenueService.GetMonthlyPayouts(year, shopId);
            return Ok(revenue);
        }
        catch (Exception ex)
        {
            return BadRequest(new Result<MonthlyPayoutsResponse>
            {
                ResultStatus = ResultStatus.Error,
                Messages = new[] { $"Error retrieving monthly payouts: {ex.Message}" }
            });
        }
    }
}