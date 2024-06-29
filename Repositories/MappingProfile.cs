using AutoMapper;
using BusinessObjects.Dtos.Account.Request;
using BusinessObjects.Dtos.Account.Response;
using BusinessObjects.Dtos.AuctionDeposits;
using BusinessObjects.Dtos.Commons;
using BusinessObjects.Dtos.Deliveries;
using BusinessObjects.Dtos.FashionItems;
using BusinessObjects.Dtos.OrderDetails;
using BusinessObjects.Dtos.Orders;
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
            CreateMap<Address, DeliveryResponse>() 
                .ForMember(a => a.Buyername, opt => opt.MapFrom(a => a.Member.Fullname))
                .ReverseMap();
            CreateMap<DeliveryRequest, Address>() .ReverseMap();
            CreateMap<FashionItemDetailRequest, FashionItem>() .ReverseMap();
            CreateMap<FashionItem, FashionItemDetailResponse>()
                .ForMember(a => a.Consigner, opt => opt.MapFrom(a => a.ConsignSaleDetail.ConsignSale.Member.Fullname))
                .ForMember(a => a.CategoryName, opt => opt.MapFrom(a => a.Category.Name))
                .ForMember(a => a.ShopAddress, opt => opt.MapFrom(a => a.Shop.Address))
                .ReverseMap();
            CreateMap<PaginationResponse<FashionItem>, PaginationResponse<FashionItemDetailResponse>>();
            CreateMap<Order, OrderResponse>() 
                .ForMember(a => a.CustomerName, opt => opt.MapFrom(a => a.Member.Fullname))
                .ForMember(a => a.RecipientName, opt => opt.MapFrom(a => a.RecipientName))
                .ForMember(a => a.ContactNumber, opt => opt.MapFrom(a => a.Phone))
                .ForMember(a => a.Address, opt => opt.MapFrom(a => a.Address))
                .ReverseMap();
            CreateMap<OrderDetail, OrderDetailResponse<FashionItemDetailResponse>>()
                .ForPath(a => a.FashionItemDetail.ShopId, opt => opt.MapFrom(a => a.FashionItem.ShopId))
                .ForPath(a => a.FashionItemDetail.ItemId, opt => opt.MapFrom(a => a.FashionItem.ItemId))
                .ForPath(a => a.FashionItemDetail.SellingPrice, opt => opt.MapFrom(a => a.FashionItem.SellingPrice))
                .ForPath(a => a.FashionItemDetail.Name, opt => opt.MapFrom(a => a.FashionItem.Name))
                .ForPath(a => a.FashionItemDetail.Note, opt => opt.MapFrom(a => a.FashionItem.Note))
                .ForPath(a => a.FashionItemDetail.Value, opt => opt.MapFrom(a => a.FashionItem.Value))
                .ForPath(a => a.FashionItemDetail.Condition, opt => opt.MapFrom(a => a.FashionItem.Condition))
                .ForPath(a => a.FashionItemDetail.ConsignDuration, opt => opt.MapFrom(a => a.FashionItem.ConsignSaleDetail.ConsignSale.ConsignDuration))
                .ForPath(a => a.FashionItemDetail.ShopAddress, opt => opt.MapFrom(a => a.FashionItem.Shop.Address))
                .ForPath(a => a.FashionItemDetail.StartDate, opt => opt.MapFrom(a => a.FashionItem.ConsignSaleDetail.ConsignSale.StartDate))
                .ForPath(a => a.FashionItemDetail.EndDate, opt => opt.MapFrom(a => a.FashionItem.ConsignSaleDetail.ConsignSale.EndDate))
                .ForPath(a => a.FashionItemDetail.Consigner, opt => opt.MapFrom(a => a.FashionItem.ConsignSaleDetail.ConsignSale.Member.Fullname))
                .ForPath(a => a.FashionItemDetail.CategoryName, opt => opt.MapFrom(a => a.FashionItem.Category.Name))
                .ForPath(a => a.FashionItemDetail.Color, opt => opt.MapFrom(a => a.FashionItem.Color))
                .ForPath(a => a.FashionItemDetail.Brand, opt => opt.MapFrom(a => a.FashionItem.Brand))
                .ReverseMap();
        }
    }
}
