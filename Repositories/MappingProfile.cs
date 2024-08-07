using AutoMapper;
using BusinessObjects.Dtos.Account.Request;
using BusinessObjects.Dtos.Account.Response;
using BusinessObjects.Dtos.Commons;
using BusinessObjects.Dtos.ConsignSaleDetails;
using BusinessObjects.Dtos.ConsignSales;
using BusinessObjects.Dtos.Deliveries;
using BusinessObjects.Dtos.FashionItems;
using BusinessObjects.Dtos.OrderDetails;
using BusinessObjects.Dtos.Orders;
using BusinessObjects.Dtos.Refunds;
using BusinessObjects.Dtos.Shops;
using BusinessObjects.Dtos.Transactions;
using BusinessObjects.Entities;

namespace Repositories
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
            CreateMap<DeliveryRequest, Address>() 
                .ForMember(a => a.Residence, opt => opt.MapFrom(a => a.Residence))
                .ReverseMap();
            CreateMap<UpdateDeliveryRequest, Address>() 
                .ReverseMap();
            CreateMap<FashionItemDetailRequest, IndividualFashionItem>() .ReverseMap();
            CreateMap<IndividualFashionItem, FashionItemDetailResponse>()
                /*.ForMember(a => a.Consigner, opt => opt.MapFrom(a => a.ConsignSaleDetail.ConsignSale.Member.Fullname))*/
                .ForMember(a => a.CategoryName, opt => opt.MapFrom(a => a.Variation.MasterItem.Category.Name))
                .ForMember(a => a.ShopAddress, opt => opt.MapFrom(a => a.Shop.Address))
                .ForMember(a => a.Images, opt => opt.MapFrom(a => a.Images.Select(c => c.Url)))
                .ReverseMap();
            CreateMap<PaginationResponse<IndividualFashionItem>, PaginationResponse<FashionItemDetailResponse>>();
            CreateMap<Order, OrderResponse>()
                .ForMember(a => a.CustomerName, opt => opt.MapFrom(a => a.Member.Fullname))
                .ForMember(a => a.RecipientName, opt => opt.MapFrom(a => a.RecipientName))
                .ForMember(a => a.ContactNumber, opt => opt.MapFrom(a => a.Phone))
                .ForMember(a => a.Address, opt => opt.MapFrom(a => a.Address))
                .ForMember(a => a.Quantity, opt => opt.MapFrom(a => a.OrderDetails.Count))
                .ForMember(a => a.OrderDetailItems, opt => opt.MapFrom(a => a.OrderDetails))
                .ForMember(a => a.Email, opt => opt.MapFrom(a => a.Email))
                .ForMember(a => a.RecipientName, opt => opt.MapFrom(a => a.RecipientName))
                .ForMember(a => a.PaymentMethod, opt => opt.MapFrom(a => a.PaymentMethod))
                .ForMember(a => a.PaymentDate, opt => opt.MapFrom(a => a.PaymentDate))
                .ForMember(a => a.CompletedDate, opt => opt.MapFrom(a => a.CompletedDate))
                .ForMember(a => a.PurchaseType, opt => opt.MapFrom(a => a.PurchaseType))
                .ForMember(a => a.OrderCode, opt => opt.MapFrom(a => a.OrderCode))
                .ForMember(a => a.Status, opt => opt.MapFrom(a => a.Status))
                .ForMember(a => a.TotalPrice, opt => opt.MapFrom(a => a.TotalPrice));
            CreateMap<OrderDetail, OrderDetailResponse<FashionItemDetailResponse>>()
                .ForPath(a => a.FashionItemDetail.ShopId, opt => opt.MapFrom(a => a.IndividualFashionItem.ShopId))
                .ForPath(a => a.FashionItemDetail.ItemId, opt => opt.MapFrom(a => a.IndividualFashionItem.ItemId))
                .ForPath(a => a.FashionItemDetail.SellingPrice, opt => opt.MapFrom(a => a.IndividualFashionItem.SellingPrice))
                .ForPath(a => a.FashionItemDetail.Name, opt => opt.MapFrom(a => a.IndividualFashionItem.Variation.MasterItem.Name))
                .ForPath(a => a.FashionItemDetail.Note, opt => opt.MapFrom(a => a.IndividualFashionItem.Note))
                /*.ForPath(a => a.FashionItemDetail.Value, opt => opt.MapFrom(a => a.FashionItem.Value))*/
                .ForPath(a => a.FashionItemDetail.Condition, opt => opt.MapFrom(a => a.IndividualFashionItem.Variation.Condition))
                /*.ForPath(a => a.FashionItemDetail.ConsignDuration, opt => opt.MapFrom(a => a.FashionItem.ConsignSaleDetail.ConsignSale.ConsignDuration))*/
                .ForPath(a => a.FashionItemDetail.ShopAddress, opt => opt.MapFrom(a => a.IndividualFashionItem.Shop.Address))
                .ForPath(a => a.FashionItemDetail.Status, opt => opt.MapFrom(a => a.IndividualFashionItem.Status))
                /*.ForPath(a => a.FashionItemDetail.Consigner, opt => opt.MapFrom(a => a.FashionItem.ConsignSaleDetail.ConsignSale.Member.Fullname))*/
                .ForPath(a => a.FashionItemDetail.CategoryName, opt => opt.MapFrom(a => a.IndividualFashionItem.Variation.MasterItem.Category.Name))
                .ForPath(a => a.FashionItemDetail.Color, opt => opt.MapFrom(a => a.IndividualFashionItem.Variation.Color))
                .ForPath(a => a.FashionItemDetail.Brand, opt => opt.MapFrom(a => a.IndividualFashionItem.Variation.MasterItem.Brand))
                .ForPath(a => a.FashionItemDetail.Gender, opt => opt.MapFrom(a => a.IndividualFashionItem.Variation.MasterItem.Gender))
                .ForPath(a => a.FashionItemDetail.Size, opt => opt.MapFrom(a => a.IndividualFashionItem.Variation.Size))
                .ReverseMap();
            CreateMap<ConsignSale, ConsignSaleResponse>()
                .ForMember(a => a.Consginer, opt => opt.MapFrom(a => a.ConsignorName))
                .ForMember(a => a.ConsignSaleDetails, opt => opt.MapFrom(a => a.ConsignSaleDetails))
                .ReverseMap();
            CreateMap<ConsignSaleDetail, ConsignSaleDetailResponse>()
                /*.ForMember(dest => dest.FashionItem, opt => opt.MapFrom(src => src.FashionItem))*/
                .ForMember(dest => dest.ConsignSaleCode, opt => opt.MapFrom(src => src.ConsignSale.ConsignSaleCode))
                .ReverseMap();
            CreateMap<Shop, ShopDetailResponse>() .ReverseMap();
            CreateMap<Refund, RefundResponse>()
                .ForMember(dest => dest.Images, opt => opt.MapFrom(src => src.Images.Select(c => c.Url)))
                .ForMember(dest => dest.OrderDetailsResponse, opt => opt.MapFrom(src => src.OrderDetail))
                .ForMember(dest => dest.CustomerName, opt => opt.MapFrom(src => src.OrderDetail.Order.Member.Fullname))
                .ForMember(dest => dest.CustomerEmail, opt => opt.MapFrom(src => src.OrderDetail.Order.Member.Email))
                .ForMember(dest => dest.CustomerPhone, opt => opt.MapFrom(src => src.OrderDetail.Order.Member.Phone))
                .ForMember(dest => dest.RefundPercentage, opt => opt.MapFrom(src => src.RefundPercentage))
                .ForMember(dest => dest.RefundAmount, opt => opt.MapFrom(src => src.RefundPercentage / 100 * src.OrderDetail.UnitPrice ))
                .ReverseMap();
            CreateMap<ConsignSaleDetail, ConsignSaleDetailResponse2>()
                /*.ForMember(dest => dest.ItemName, opt => opt.MapFrom(src => src.FashionItem.Name))*/
                .ReverseMap();
            CreateMap<OrderDetail, OrderDetailsResponse>()
                .ForMember(dest => dest.ItemName, opt => opt.MapFrom(src => src.IndividualFashionItem.Variation.MasterItem.Name))
                .ForMember(dest => dest.ItemStatus, opt => opt.MapFrom(src => src.IndividualFashionItem.Status))
                .ForMember(dest => dest.ItemBrand, opt => opt.MapFrom(src => !string.IsNullOrEmpty(src.IndividualFashionItem.Variation.MasterItem.Brand) ? src.IndividualFashionItem.Variation.MasterItem.Brand : "No Brand"))
                .ForMember(dest => dest.ItemColor, opt => opt.MapFrom(src => src.IndividualFashionItem.Variation.Color))
                .ForMember(dest => dest.Condition, opt => opt.MapFrom(src => src.IndividualFashionItem.Variation.Condition))
                .ForMember(dest => dest.ItemGender, opt => opt.MapFrom(src => src.IndividualFashionItem.Variation.MasterItem.Gender))
                .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.IndividualFashionItem.Variation.MasterItem.Name))
                .ForMember(dest => dest.ItemNote, opt => opt.MapFrom(src => src.IndividualFashionItem.Note))
                .ForMember(dest => dest.ItemType, opt => opt.MapFrom(src => src.IndividualFashionItem.Type))
                .ForMember(dest => dest.ItemSize, opt => opt.MapFrom(src => src.IndividualFashionItem.Variation.Size))
                .ForMember(dest => dest.ItemImage, opt => opt.MapFrom(src => src.IndividualFashionItem.Images.Select(c => c.Url)))
                .ForMember(dest => dest.CreatedDate, opt => opt.MapFrom(src => src.CreatedDate))
                .ForMember(dest => dest.ShopAddress, opt => opt.MapFrom(src => src.IndividualFashionItem.Shop.Address))
                .ForMember(dest => dest.ShopId, opt => opt.MapFrom(src => src.IndividualFashionItem.Shop.ShopId))
                .ForMember(dest => dest.OrderCode, opt => opt.MapFrom(src => src.Order.OrderCode))
                .ReverseMap();
            CreateMap<Transaction, GetTransactionsResponse>()
                /*.ForMember(dest => dest.ItemName, opt => opt.MapFrom(src => src.FashionItem.Name))*/
                .ReverseMap();
        }
    }
}
