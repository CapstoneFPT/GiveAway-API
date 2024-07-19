using BusinessObjects.Entities;

namespace Repositories.Withdraws;

public interface IWithdrawRepository
{
    Task<Withdraw> CreateWithdraw(Withdraw withdraw);
}