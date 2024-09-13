using System;
using BusinessObjects.Dtos.Commons;
using Dao;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Quartz;

namespace Services.Recharges;

public class RechargeExpirationJob : IJob
{
  private readonly IServiceProvider _serviceProvider;
  private readonly ILogger<RechargeExpirationJob> _logger;

  public RechargeExpirationJob(IServiceProvider serviceProvider, ILogger<RechargeExpirationJob> logger)

  {
    _serviceProvider = serviceProvider;
    _logger = logger;
  }

    public async Task Execute(IJobExecutionContext context)
    {
      using var scope = _serviceProvider.CreateScope();
      var dbContext = scope.ServiceProvider.GetRequiredService<GiveAwayDbContext>();

      var dataMap = context.JobDetail.JobDataMap;
      var rechargeId = dataMap.GetGuid("RechargeId");

      _logger.LogInformation("Executing RechargeExpirationJob for RechargeId: {RechargeId}", rechargeId);

      try
      {
        var recharge = await dbContext.Recharges.FirstOrDefaultAsync(r => r.RechargeId == rechargeId);

        if (recharge == null)
        {
          _logger.LogWarning("Recharge {RechargeId} not found", rechargeId);
          return;
        }

        if (recharge.Status == RechargeStatus.Failed)
        {
          _logger.LogInformation("Recharge {RechargeId} has already failed", rechargeId);
          return;
        }

        recharge.Status = RechargeStatus.Cancelled;
        await dbContext.SaveChangesAsync();

        _logger.LogInformation("Recharge {RechargeId} has been cancelled", rechargeId);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error executing RechargeExpirationJob for RechargeId: {RechargeId}", rechargeId);
      }
    }
}
