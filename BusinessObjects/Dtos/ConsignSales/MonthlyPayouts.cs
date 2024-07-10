namespace BusinessObjects.Dtos.ConsignSales;

public class MonthlyPayoutsResponse
{
   public int Year { get; set; }
   public List<MonthPayout> MonthlyPayouts { get; set; } = new List<MonthPayout>();
}

public class MonthPayout
{
   public int Month { get; set; }
   public decimal ConsignPayout { get; set; }
}