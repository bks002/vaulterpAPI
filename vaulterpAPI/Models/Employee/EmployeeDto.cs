namespace vaulterpAPI.Models.Employee
{
    public class EmployeeDto
    {
        public int? EmployeeId { get; set; }
        public string? EmployeeCode { get; set; }
        public string? EmployeeName { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public int? OfficeId { get; set; }
        public string? Department { get; set; }
        public string? Designation { get; set; }
        public int? RoleId { get; set; }
        public int? ReportsTo { get; set; }
        public DateTime? JoiningDate { get; set; }
        public DateTime? LeavingDate { get; set; }
        public bool IsActive { get; set; } = true;
        public string? ProfileImageUrl { get; set; }

        public string? EmploymentType { get; set; }
        public string? DateOfBirth { get; set; }
        public string? PanCard { get; set; }
        public string? AadharCard { get; set; }
        public string? Address1 { get; set; }
        public string? Address2 { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? Gender { get; set; }

        public int? CreatedBy { get; set; }
        public int? ModifiedBy { get; set; }

        public string? OfficeName { get; set; }
        public string? Latitude { get; set; }

        public string? Longitude { get; set; }

        public string? Username { get; set; }
    }


    public class OperationDto
    {
        public int OperationId { get; set; }
        public string OperationName { get; set; }
        public string Description { get; set; }
        public int OfficeId { get; set; }
        public bool IsActive { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public int? UpdatedBy { get; set; }
        public DateTime? UpdatedOn { get; set; }
    }

    // ✅ DTO for mapping
    public class EmpOpsDto
    {
        public int EmployeeId { get; set; }
        public int OperationId { get; set; }
        public int? CreatedBy { get; set; }
        public int? UpdatedBy { get; set; }
    }

    public class EmployeeOperationMappingRequest
    {
        public int EmployeeId { get; set; }
        public List<int> OperationIds { get; set; } = new();
        public int UpdatedBy { get; set; }
    }
    public class CreateOperationRequest
    {
        public string OperationName { get; set; } = "";
        public string? Description { get; set; }
        public int OfficeId { get; set; }
        public int CreatedBy { get; set; }
    }

    public class EmployeeShiftDto
    {
        public int EmployeeId { get; set; }
        public string? EmployeeName { get; set; } // for response
        public int ShiftId { get; set; }
        public string? ShiftName { get; set; } // for response
        public DateTime DateFrom { get; set; }
        public DateTime DateTo { get; set; }
        public bool IsActive { get; set; } = true;
        public string? MobileNo { get; set; }
        public int CreatedBy { get; set; }
        public int? UpdatedBy { get; set; }
    }
}
