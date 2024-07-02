namespace BusinessObjects.Dtos.Shops;

public class ShopDetailResponse
{
    public Guid ShopId { get; set; }
    public string Address { get; set; }
    public Guid StaffId { get; set; }
    public string Phone {  get; set; }
    /*public StaffDetailResponse? Staff { get; set; }*/
}

public class StaffDetailResponse
{
    public Guid StaffId { get; set; }
    public string Name { get; set; }
    public string Phone { get; set; }
}