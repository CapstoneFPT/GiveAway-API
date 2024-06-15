using AutoMapper;
using BusinessObjects.Dtos.Account.Request;
using BusinessObjects.Dtos.Account.Response;
using BusinessObjects.Dtos.AuctionDeposits;
using BusinessObjects.Dtos.Deliveries;
using BusinessObjects.Dtos.Wallet;
using BusinessObjects.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Account, AccountResponse>()
                .ReverseMap();
            CreateMap<UpdateAccountRequest, Account>() .ReverseMap();
            CreateMap<Wallet, WalletResponse>()
                .ForMember(a => a.AccountName, opt => opt.MapFrom(a => a.Member.Fullname))
                .ReverseMap();
            CreateMap<UpdateWalletRequest, Wallet>() .ReverseMap();
            CreateMap<Delivery, DeliveryResponse>() 
                .ForMember(a => a.Buyername, opt => opt.MapFrom(a => a.Member.Fullname))
                .ReverseMap();
            CreateMap<DeliveryRequest, Delivery>() .ReverseMap();
        }
    }
}
