using Microsoft.AspNetCore.Mvc;
using Npgsql;
using vaulterpAPI.Models.Employee;

namespace vaulterpAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EmployeeController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public EmployeeController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        private string GetConnectionString() =>
            _configuration.GetConnectionString("DefaultConnection");

        // ✅ Get all employees (optionally filtered by office ID)
        [HttpGet]
        public IActionResult GetAll([FromQuery] int? officeId)
        {
            try
            {
                var employees = new List<EmployeeDto>();
                var query = "SELECT * FROM master.employee_master WHERE is_active = true";
                if (officeId.HasValue)
                    query += " AND office_id = @office_id";

                using var conn = new NpgsqlConnection(GetConnectionString());
                using var cmd = new NpgsqlCommand(query, conn);
                if (officeId.HasValue)
                    cmd.Parameters.AddWithValue("@office_id", officeId.Value);

                conn.Open();
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    employees.Add(new EmployeeDto
                    {
                        EmployeeId = (int)reader["employee_id"],
                        EmployeeCode = reader["employee_code"]?.ToString(),
                        EmployeeName = reader["employee_name"]?.ToString(),
                        Email = reader["email"]?.ToString(),
                        PhoneNumber = reader["phone_number"]?.ToString(),
                        OfficeId = (int?)reader["office_id"],
                        Department = reader["department"]?.ToString(),
                        Designation = reader["designation"]?.ToString(),
                        RoleId = (int?)reader["role_id"],
                        ReportsTo = reader["reports_to"] as int?,
                        JoiningDate = reader["joining_date"] as DateTime?,
                        LeavingDate = reader["leaving_date"] as DateTime?,
                        IsActive = (bool)reader["is_active"],
                        ProfileImageUrl = reader["profile_image_url"]?.ToString(),
                        EmploymentType = reader["employement_type"]?.ToString(),
                        DateOfBirth = reader["date_of_birth"]?.ToString(),
                        PanCard = reader["pan_card"]?.ToString(),
                        AadharCard = reader["aadhar_card"]?.ToString(),
                        Address1 = reader["address_1"]?.ToString(),
                        Address2 = reader["address_2"]?.ToString(),
                        City = reader["city"]?.ToString(),
                        State = reader["state"]?.ToString(),
                        Gender = reader["gender"]?.ToString()
                    });
                }

                return Ok(employees);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error fetching employees", error = ex.Message });
            }
        }

        // ✅ GET employee by ID
        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            try
            {
                using var conn = new NpgsqlConnection(GetConnectionString());
                using var cmd = new NpgsqlCommand("SELECT em.*, om.latitude, om.longitude FROM master.employee_master em  LEFT JOIN master.office_master om ON em.office_id = om.office_id  WHERE employee_id = @id AND em.is_active = true", conn);
                cmd.Parameters.AddWithValue("@id", id);

                conn.Open();
                using var reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    var employee = new EmployeeDto
                    {
                        EmployeeId = (int)reader["employee_id"],
                        EmployeeCode = reader["employee_code"]?.ToString(),
                        EmployeeName = reader["employee_name"]?.ToString(),
                        Email = reader["email"]?.ToString(),
                        PhoneNumber = reader["phone_number"]?.ToString(),
                        OfficeId = (int?)reader["office_id"],
                        Department = reader["department"]?.ToString(),
                        Designation = reader["designation"]?.ToString(),
                        RoleId = (int?)reader["role_id"],
                        ReportsTo = reader["reports_to"] as int?,
                        JoiningDate = reader["joining_date"] as DateTime?,
                        LeavingDate = reader["leaving_date"] as DateTime?,
                        IsActive = (bool)reader["is_active"],
                        ProfileImageUrl = reader["profile_image_url"]?.ToString(),
                        EmploymentType = reader["employement_type"]?.ToString(),
                        DateOfBirth = reader["date_of_birth"]?.ToString(),
                        PanCard = reader["pan_card"]?.ToString(),
                        AadharCard = reader["aadhar_card"]?.ToString(),
                        Address1 = reader["address_1"]?.ToString(),
                        Address2 = reader["address_2"]?.ToString(),
                        City = reader["city"]?.ToString(),
                        State = reader["state"]?.ToString(),
                        Gender = reader["gender"]?.ToString(),
                        Latitude = reader["latitude"]?.ToString(),
                        Longitude = reader["longitude"]?.ToString(),
                    };

                    return Ok(employee);
                }

                return NotFound(new { message = "Employee not found" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving employee", error = ex.Message });
            }
        }


        // ✅ Create employee
        [HttpPost]
        public IActionResult Create(EmployeeDto emp)
        {
            try
            {
                using var conn = new NpgsqlConnection(GetConnectionString());
                using var cmd = new NpgsqlCommand(@"
                    INSERT INTO master.employee_master (
                        employee_code, employee_name, email, phone_number, office_id, department, designation, role_id, reports_to,
                        joining_date, leaving_date, profile_image_url, is_active, created_by, created_on,
                        employement_type, date_of_birth, pan_card, aadhar_card, address_1, address_2, city, state, gender
                    ) VALUES (
                        @employee_code, @employee_name, @email, @phone_number, @office_id, @department, @designation, @role_id, @reports_to,
                        @joining_date, @leaving_date, @profile_image_url, @is_active, @created_by, NOW(),
                        @employment_type, @dob, @pan, @aadhar, @address1, @address2, @city, @state, @gender
                    )", conn);

                cmd.Parameters.AddWithValue("@employee_code", emp.EmployeeCode ?? "");
                cmd.Parameters.AddWithValue("@employee_name", emp.EmployeeName ?? "");
                cmd.Parameters.AddWithValue("@email", emp.Email ?? "");
                cmd.Parameters.AddWithValue("@phone_number", emp.PhoneNumber ?? "");
                cmd.Parameters.AddWithValue("@office_id", emp.OfficeId ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@department", emp.Department ?? "");
                cmd.Parameters.AddWithValue("@designation", emp.Designation ?? "");
                cmd.Parameters.AddWithValue("@role_id", emp.RoleId ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@reports_to", emp.ReportsTo ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@joining_date", emp.JoiningDate ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@leaving_date", emp.LeavingDate ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@profile_image_url", emp.ProfileImageUrl ?? "");
                cmd.Parameters.AddWithValue("@is_active", emp.IsActive);
                cmd.Parameters.AddWithValue("@created_by", emp.CreatedBy ?? 1);
                cmd.Parameters.AddWithValue("@employment_type", emp.EmploymentType ?? "");
                cmd.Parameters.AddWithValue("@dob", emp.DateOfBirth ?? "");
                cmd.Parameters.AddWithValue("@pan", emp.PanCard ?? "");
                cmd.Parameters.AddWithValue("@aadhar", emp.AadharCard ?? "");
                cmd.Parameters.AddWithValue("@address1", emp.Address1 ?? "");
                cmd.Parameters.AddWithValue("@address2", emp.Address2 ?? "");
                cmd.Parameters.AddWithValue("@city", emp.City ?? "");
                cmd.Parameters.AddWithValue("@state", emp.State ?? "");
                cmd.Parameters.AddWithValue("@gender", emp.Gender ?? "");

                conn.Open();
                cmd.ExecuteNonQuery();

                return Ok(new { message = "Employee created successfully", status = true });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error creating employee", error = ex.Message });
            }
        }

        // ✅ Update employee
        [HttpPut("{id}")]
        public IActionResult Update(int id, EmployeeDto emp)
        {
            try
            {
                using var conn = new NpgsqlConnection(GetConnectionString());
                using var cmd = new NpgsqlCommand(@"
                    UPDATE master.employee_master SET
                        employee_name = @employee_name,
                        email = @email,
                        phone_number = @phone_number,
                        department = @department,
                        designation = @designation,
                        role_id = @role_id,
                        reports_to = @reports_to,
                        joining_date = @joining_date,
                        leaving_date = @leaving_date,
                        profile_image_url = @profile_image_url,
                        modified_by = @modified_by,
                        modified_on = NOW(),
                        employement_type = @employment_type,
                        date_of_birth = @dob,
                        pan_card = @pan,
                        aadhar_card = @aadhar,
                        address_1 = @address1,
                        address_2 = @address2,
                        city = @city,
                        state = @state,
                        gender = @gender
                    WHERE employee_id = @employee_id", conn);

                cmd.Parameters.AddWithValue("@employee_id", id);
                cmd.Parameters.AddWithValue("@employee_name", emp.EmployeeName ?? "");
                cmd.Parameters.AddWithValue("@email", emp.Email ?? "");
                cmd.Parameters.AddWithValue("@phone_number", emp.PhoneNumber ?? "");
                cmd.Parameters.AddWithValue("@department", emp.Department ?? "");
                cmd.Parameters.AddWithValue("@designation", emp.Designation ?? "");
                cmd.Parameters.AddWithValue("@role_id", emp.RoleId ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@reports_to", emp.ReportsTo ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@joining_date", emp.JoiningDate ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@leaving_date", emp.LeavingDate ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@profile_image_url", emp.ProfileImageUrl ?? "");
                cmd.Parameters.AddWithValue("@modified_by", emp.ModifiedBy ?? 1);
                cmd.Parameters.AddWithValue("@employment_type", emp.EmploymentType ?? "");
                cmd.Parameters.AddWithValue("@dob", emp.DateOfBirth ?? "");
                cmd.Parameters.AddWithValue("@pan", emp.PanCard ?? "");
                cmd.Parameters.AddWithValue("@aadhar", emp.AadharCard ?? "");
                cmd.Parameters.AddWithValue("@address1", emp.Address1 ?? "");
                cmd.Parameters.AddWithValue("@address2", emp.Address2 ?? "");
                cmd.Parameters.AddWithValue("@city", emp.City ?? "");
                cmd.Parameters.AddWithValue("@state", emp.State ?? "");
                cmd.Parameters.AddWithValue("@gender", emp.Gender ?? "");

                conn.Open();
                cmd.ExecuteNonQuery();

                return Ok(new { message = "Employee updated successfully", status = true });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error updating employee", error = ex.Message });
            }
        }

        // ✅ Soft delete employee
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            try
            {
                using var conn = new NpgsqlConnection(GetConnectionString());
                using var cmd = new NpgsqlCommand("UPDATE master.employee_master SET is_active = FALSE WHERE employee_id = @id", conn);
                cmd.Parameters.AddWithValue("@id", id);

                conn.Open();
                cmd.ExecuteNonQuery();

                return Ok(new { message = "Employee deleted (soft) successfully", status = true });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error deleting employee", error = ex.Message });
            }
        }

        [HttpGet("getBy/{imageName}")]
        public IActionResult GetByImage(string imageName)
        {
            try
            {
                using var conn = new NpgsqlConnection(GetConnectionString());
                using var cmd = new NpgsqlCommand(@"
            SELECT 
                em.*, 
                om.office_name, 
                om.latitude, 
                om.longitude,
                u.username 
            FROM master.employee_master em 
            LEFT JOIN master.office_master om ON em.office_id = om.office_id 
            LEFT JOIN identity.user u ON u.employee_id = em.employee_id 
            WHERE em.profile_image_name = @imageName  
              AND em.is_active = true;", conn);

                cmd.Parameters.AddWithValue("@imageName", imageName);

                conn.Open();
                using var reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    string baseUrl = "http://43.230.64.37:8000/images/";
                    string profileImageName = reader["profile_image_name"] != DBNull.Value ? reader["profile_image_name"].ToString() : null;
                    string profileImageUrl = profileImageName != null ? baseUrl + profileImageName : null;

                    var employee = new EmployeeDto
                    {
                        EmployeeId = (int)reader["employee_id"],
                        EmployeeCode = reader["employee_code"]?.ToString(),
                        EmployeeName = reader["employee_name"]?.ToString(),
                        Email = reader["email"]?.ToString(),
                        PhoneNumber = reader["phone_number"]?.ToString(),
                        OfficeId = (int?)reader["office_id"],
                        Department = reader["department"]?.ToString(),
                        Designation = reader["designation"]?.ToString(),
                        RoleId = (int?)reader["role_id"],
                        ReportsTo = reader["reports_to"] as int?,
                        JoiningDate = reader["joining_date"] as DateTime?,
                        LeavingDate = reader["leaving_date"] as DateTime?,
                        IsActive = (bool)reader["is_active"],
                        ProfileImageUrl = profileImageUrl,
                        EmploymentType = reader["employement_type"]?.ToString(),
                        DateOfBirth = reader["date_of_birth"]?.ToString(),
                        PanCard = reader["pan_card"]?.ToString(),
                        AadharCard = reader["aadhar_card"]?.ToString(),
                        Address1 = reader["address_1"]?.ToString(),
                        Address2 = reader["address_2"]?.ToString(),
                        City = reader["city"]?.ToString(),
                        State = reader["state"]?.ToString(),
                        Gender = reader["gender"]?.ToString(),
                        OfficeName = reader["office_name"]?.ToString(),
                        Latitude = reader["latitude"]?.ToString(),
                        Longitude = reader["longitude"]?.ToString(),
                        Username = reader["username"]?.ToString() // ✅ Add this line
                    };

                    return Ok(employee);
                }

                return NotFound(new { message = "Employee not found" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving employee", error = ex.Message });
            }
        }

    }
}
