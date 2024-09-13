using Quartz;
using Microsoft.Extensions.DependencyInjection;
using Repositories.Withdraws;
using Repositories.Accounts;
using BusinessObjects.Dtos.Commons;
using Microsoft.EntityFrameworkCore;

namespace Services.Withdraws;

public class WithdrawExpirationJob : IJob
{
    private readonly IServiceProvider _serviceProvider;

    public WithdrawExpirationJob(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        using var scope = _serviceProvider.CreateScope();
        var withdrawRepository = scope.ServiceProvider.GetRequiredService<IWithdrawRepository>();
        var accountRepository = scope.ServiceProvider.GetRequiredService<IAccountRepository>();

        var dataMap = context.JobDetail.JobDataMap;
        var withdrawId = dataMap.GetGuid("WithdrawId");

        var withdraw = await withdrawRepository.GetSingleWithdraw(w => w.WithdrawId == withdrawId);

        if (withdraw == null || withdraw.Status != WithdrawStatus.Processing)
        {
            return;
        }

        withdraw.Status = WithdrawStatus.Expired;
        await withdrawRepository.UpdateWithdraw(withdraw);

        var member = await accountRepository.GetMemberById(withdraw.MemberId);
        if (member != null)
        {
            member.Balance += withdraw.Amount;
            await accountRepository.UpdateAccount(member);
        }
    }
}