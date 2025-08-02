namespace vaulterpAPI.Models.Attendance
{
    public class ShiftDto
    {
        public int ShiftId { get; set; }
        public string? ShiftName { get; set; }
        public string? ShiftCode { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public int OfficeId { get; set; }
    }

}
