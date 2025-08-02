namespace vaulterpAPI.Models.Asset
{
    public class AssetDto
    {
        public int AssetId { get; set; }
        public string AssetCode { get; set; }
        public string AssetName { get; set; }
        public int AssetTypeId { get; set; }
        public int OfficeId { get; set; }
        public string? ModelNumber { get; set; }
        public string? SerialNumber { get; set; }
        public DateTime? PurchaseDate { get; set; }
        public DateTime? WarrantyExpiry { get; set; }
        public string? Manufacturer { get; set; }
        public string? Supplier { get; set; }
        public bool IsActive { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string? CreatedBy { get; set; }
    }

    public class AssetOperationMappingRequest
    {
        public int AssetId { get; set; }
        public List<int> OperationIds { get; set; } = new();
        public int UpdatedBy { get; set; }
    }

}
