namespace vaulterpAPI.Models.Identity
{
    public class UserDto
    {
        public int Id { get; set; }
        public string? Email { get; set; }
        public string? PasswordHash { get; set; }
        public int UsertypeId { get; set; }
        public int EmployeeId { get; set; }
        public int? OfficeId { get; set; }
        public bool IsFirstLogin { get; set; } = true;
        public bool IsActive { get; set; } = true;
        public DateTime? LastLogin { get; set; }
        public DateTime CreatedOn { get; set; }
    }
}
