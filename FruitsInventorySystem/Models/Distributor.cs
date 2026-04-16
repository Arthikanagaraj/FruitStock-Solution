namespace FruitsInventorySystem.Models
{
    public class Distributor
    {
        public int Id { get; set; }   // Primary Key (VERY IMPORTANT)

        public string? DistributorId { get; set; }
        public string? DistributorName { get; set; }
        public string? PhoneNumber { get; set; }

        public DateTime? Date { get; set; }

        public string? FruitId { get; set; }
        public string? FruitName { get; set; }
        public string? Quality { get; set; }

        public int? BoxCount { get; set; }
        public decimal? PricePerBox { get; set; }
        public decimal? Total { get; set; }

        public decimal? GrandTotal { get; set; }
        public decimal? PaidAmount { get; set; }
        public decimal? RemainingAmount { get; set; }

        public string? PaymentType { get; set; }
    }
}
