namespace vaulterpAPI.Models
{
    public class PODto
    {
        public int PurchaseOrderId { get; set; }
        public string PONumber { get; set; }
        public DateTime PODateTime { get; set; }
        public string? BillingAddress { get; set; }
        public string? ShippingAddress { get; set; }
        public int OfficeId { get; set; }
        public bool IsApproved { get; set; }

        public string? CreatedBy { get; set; }
        public int VendorId { get; set; }
        public string VendorName { get; set; }
        public string ContactPerson { get; set; }
        public string ContactNumber { get; set; }
        public string Email { get; set; }

        public List<POItemDto> Items { get; set; } = new List<POItemDto>();
    }

    public class POItemDto
    {
        public int PurchaseOrderItemId { get; set; }
        public int ItemId { get; set; }
        public string ItemName { get; set; }
        public decimal Quantity { get; set; }
        public decimal Rate { get; set; }
        public decimal LineTotal { get; set; }
    }
}
