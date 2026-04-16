namespace FruitsInventorySystem.Models
{
    public class Supplier
    {
        public int Id { get; set; }   // 🔴 MAIN PRIMARY KEY

        public string SupplierId { get; set; }
        public string SupplierName { get; set; }
        public string PhoneNumber { get; set; }
        public DateTime? Date { get; set; }

        public string FruitId { get; set; }
        public string FruitName { get; set; }
        public string Quality { get; set; }
        public int BoxCount { get; set; }
        public decimal PricePerBox { get; set; }
        public decimal Total { get; set; }

        public decimal GrandTotal { get; set; }
        public decimal PaidAmount { get; set; }
        public decimal RemainingAmount { get; set; }

        public string PaymentType { get; set; }
    }
}
