using AutoMapper;
using BusinessObjects.Dtos.Account.Response;
using BusinessObjects.Dtos.Commons;
using BusinessObjects.Dtos.Wallet;
using BusinessObjects.Entities;
using Repositories.Wallets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Wallets
{
    public class WalletService : IWalletService
    {
        private readonly IWalletRepository _walletRepository;
        private readonly IMapper _mapper;

        public WalletService(IWalletRepository walletRepository, IMapper mapper)
        {
            _walletRepository = walletRepository;
            _mapper = mapper;
        }

        public async Task<Result<WalletResponse>> GetWalletByAccountId(Guid accountId)
        {
            return new Result<WalletResponse>()
            {
                Data = _mapper.Map<WalletResponse>(await _walletRepository.GetWalletByAccountId(accountId)),
                Messages = ["Successfully"],
                ResultStatus = ResultStatus.Success
            };
        }

        public async Task<Result<WalletResponse>> UpdateWallet(Guid id, UpdateWalletRequest request)
        {
            var response = new Result<WalletResponse>();
            var wallet = await _walletRepository.GetWalletByAccountId(id);
            if (wallet == null)
            {
                response.Messages = ["Wallet is not found!"];
                response.ResultStatus = ResultStatus.NotFound;
                return response;
            }
            else if (request.BankAccountNumber.Equals(wallet.BankAccountNumber) && request.BankName.Equals(wallet.BankName))
            {
                response.Data = _mapper.Map<WalletResponse>(wallet);
                response.Messages = ["Nothing change!"];
                response.ResultStatus = ResultStatus.Error;
                return response;
            }
            var newdata = _mapper.Map(request, wallet);
            response.Data = _mapper.Map<WalletResponse>(await _walletRepository.UpdateWallet(newdata));
            response.Messages = ["Update successfully"];
            response.ResultStatus = ResultStatus.Success;
            return response;
        }
    }
}
