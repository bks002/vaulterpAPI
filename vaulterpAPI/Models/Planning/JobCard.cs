namespace vaulterpAPI.Models.Planning
{
    public class JobCard
    {
        public int Id { get; set; }
        public string OrderNo { get; set; }
        public string IsCode { get; set; }
        public DateTime Date { get; set; }
        public int AssetId { get; set; }
        public int ShiftId { get; set; }
        public int OperationId { get; set; }
        public int? Size { get; set; }
        public string NoDiaOfStands { get; set; }
        public string Shape { get; set; }
        public bool IsCompacted { get; set; }
        public string Compound { get; set; }
        public string Color { get; set; }
        public string Thickness { get; set; }
        public string Length { get; set; }
        public string NoDiaOfAmWire { get; set; }
        public string PayOffDno { get; set; }
        public string TakeUpDrumSize { get; set; }
        public string Embrossing { get; set; }
        public string Remark { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public int? UpdatedBy { get; set; }
        public DateTime? UpdatedOn { get; set; }
        public int OfficeId { get; set; }
    }
}