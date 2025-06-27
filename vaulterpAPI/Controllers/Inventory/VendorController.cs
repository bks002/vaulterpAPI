using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using vaulterpAPI.Models;

namespace vaulterpAPI.Controllers.Inventory
{
    [ApiController]
    [Route("api/inventory/[controller]")]
    public class VendorController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public VendorController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        private string GetConnectionString() =>
            _configuration.GetConnectionString("DefaultConnection");

        [HttpGet]
        public async Task<IActionResult> GetAllVendors(int officeId)
        {
            var vendors = new List<VendorDto>();

            using var conn = new SqlConnection(GetConnectionString());
            using var cmd = new SqlCommand(@"SELECT Id, OfficeId, Name, ContactPerson, ContactNumber, Email, Address, 
                                              GSTNumber, PANNumber, IsApproved, ApprovedBy, CreatedBy, CreatedOn, IsActive 
                                              FROM Inventory.Vendor 
                                              WHERE OfficeId = @OfficeId AND IsActive = 1 AND IsApproved = 1", conn);
            cmd.Parameters.AddWithValue("@OfficeId", officeId);

            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                vendors.Add(new VendorDto
                {
                    Id = reader.GetInt32(0),
                    OfficeId = reader.GetInt32(1),
                    Name = reader.GetString(2),
                    ContactPerson = reader.IsDBNull(3) ? null : reader.GetString(3),
                    ContactNumber = reader.IsDBNull(4) ? null : reader.GetString(4),
                    Email = reader.IsDBNull(5) ? null : reader.GetString(5),
                    Address = reader.IsDBNull(6) ? null : reader.GetString(6),
                    GSTNumber = reader.IsDBNull(7) ? null : reader.GetString(7),
                    PANNumber = reader.IsDBNull(8) ? null : reader.GetString(8),
                    IsApproved = reader.GetBoolean(9),
                    ApprovedBy = reader.IsDBNull(10) ? (int?)null : reader.GetInt32(10),
                    CreatedBy = reader.GetInt32(11),
                    CreatedOn = reader.GetDateTime(12),
                    IsActive = reader.GetBoolean(13),
                });
            }

            return vendors.Count > 0 ? Ok(vendors) : NotFound();
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetVendorById(int id)
        {
            using var conn = new SqlConnection(GetConnectionString());
            using var cmd = new SqlCommand(@"SELECT Id, OfficeId, Name, ContactPerson, ContactNumber, Email, Address,
                                              GSTNumber, PANNumber, IsApproved, ApprovedBy, CreatedBy, CreatedOn, IsActive
                                              FROM Inventory.Vendor 
                                              WHERE Id = @Id", conn);
            cmd.Parameters.AddWithValue("@Id", id);

            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                var vendor = new VendorDto
                {
                    Id = reader.GetInt32(0),
                    OfficeId = reader.GetInt32(1),
                    Name = reader.GetString(2),
                    ContactPerson = reader.IsDBNull(3) ? null : reader.GetString(3),
                    ContactNumber = reader.IsDBNull(4) ? null : reader.GetString(4),
                    Email = reader.IsDBNull(5) ? null : reader.GetString(5),
                    Address = reader.IsDBNull(6) ? null : reader.GetString(6),
                    GSTNumber = reader.IsDBNull(7) ? null : reader.GetString(7),
                    PANNumber = reader.IsDBNull(8) ? null : reader.GetString(8),
                    IsApproved = reader.GetBoolean(9),
                    ApprovedBy = reader.IsDBNull(10) ? (int?)null : reader.GetInt32(10),
                    CreatedBy = reader.GetInt32(11),
                    CreatedOn = reader.GetDateTime(12),
                    IsActive = reader.GetBoolean(13),
                };
                return Ok(vendor);
            }

            return NotFound();
        }

        [HttpPost]
        public async Task<IActionResult> CreateVendor([FromBody] VendorDto dto)
        {
            if (dto == null) return BadRequest("Invalid vendor data.");
            using var conn = new SqlConnection(GetConnectionString());

            var query = @"INSERT INTO Inventory.Vendor 
                          (OfficeId, Name, ContactPerson, ContactNumber, Email, Address, GSTNumber, PANNumber, CreatedBy, CreatedOn, IsActive, IsApproved)
                          VALUES (@OfficeId, @Name, @ContactPerson, @ContactNumber, @Email, @Address, @GSTNumber, @PANNumber, @CreatedBy, GETDATE(), 1, 1);
                          SELECT SCOPE_IDENTITY();";

            using var cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@OfficeId", dto.OfficeId);
            cmd.Parameters.AddWithValue("@Name", dto.Name);
            cmd.Parameters.AddWithValue("@ContactPerson", (object?)dto.ContactPerson ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@ContactNumber", (object?)dto.ContactNumber ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Email", (object?)dto.Email ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Address", (object?)dto.Address ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@GSTNumber", (object?)dto.GSTNumber ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@PANNumber", (object?)dto.PANNumber ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@CreatedBy", dto.CreatedBy);

            await conn.OpenAsync();
            var insertedId = Convert.ToInt32(await cmd.ExecuteScalarAsync());

            return Ok(new { message = "Vendor created successfully", Id = insertedId });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateVendor(int id, [FromBody] VendorDto dto)
        {
            if (dto == null) return BadRequest("Invalid vendor data.");
            using var conn = new SqlConnection(GetConnectionString());

            var query = @"UPDATE Inventory.Vendor 
                          SET Name = @Name,
                              ContactPerson = @ContactPerson,
                              ContactNumber = @ContactNumber,
                              Email = @Email,
                              Address = @Address,
                              GSTNumber = @GSTNumber,
                              PANNumber = @PANNumber,
                              IsApproved = @IsApproved,
                              ApprovedBy = @ApprovedBy
                          WHERE Id = @Id";

            using var cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@Id", id);
            cmd.Parameters.AddWithValue("@Name", dto.Name);
            cmd.Parameters.AddWithValue("@ContactPerson", (object?)dto.ContactPerson ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@ContactNumber", (object?)dto.ContactNumber ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Email", (object?)dto.Email ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Address", (object?)dto.Address ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@GSTNumber", (object?)dto.GSTNumber ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@PANNumber", (object?)dto.PANNumber ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@IsApproved", dto.IsApproved);
            cmd.Parameters.AddWithValue("@ApprovedBy", (object?)dto.ApprovedBy ?? DBNull.Value);

            await conn.OpenAsync();
            var rowsAffected = await cmd.ExecuteNonQueryAsync();

            return rowsAffected > 0
                ? Ok(new { message = "Vendor updated successfully" })
                : NotFound();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteVendor(int id)
        {
            using var conn = new SqlConnection(GetConnectionString());

            var query = @"UPDATE Inventory.Vendor 
                          SET IsActive = 0 
                          WHERE Id = @Id AND IsActive = 1";

            using var cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@Id", id);

            await conn.OpenAsync();
            var rowsAffected = await cmd.ExecuteNonQueryAsync();

            return rowsAffected > 0
                ? Ok(new { message = "Vendor deleted (soft) successfully" })
                : NotFound();
        }

        [HttpGet("pendingApproval")]
        public async Task<IActionResult> GetPendingApprovalVendors(int officeId)
        {
            var pendingVendors = new List<VendorDto>();

            using var conn = new SqlConnection(GetConnectionString());
            using var cmd = new SqlCommand(@"SELECT Id, OfficeId, Name, ContactPerson, ContactNumber, Email, Address,
                                              GSTNumber, PANNumber, IsApproved, ApprovedBy, CreatedBy, CreatedOn, IsActive
                                              FROM Inventory.Vendor 
                                              WHERE OfficeId = @OfficeId AND IsActive = 1 AND IsApproved = 0", conn);
            cmd.Parameters.AddWithValue("@OfficeId", officeId);

            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                pendingVendors.Add(new VendorDto
                {
                    Id = reader.GetInt32(0),
                    OfficeId = reader.GetInt32(1),
                    Name = reader.GetString(2),
                    ContactPerson = reader.IsDBNull(3) ? null : reader.GetString(3),
                    ContactNumber = reader.IsDBNull(4) ? null : reader.GetString(4),
                    Email = reader.IsDBNull(5) ? null : reader.GetString(5),
                    Address = reader.IsDBNull(6) ? null : reader.GetString(6),
                    GSTNumber = reader.IsDBNull(7) ? null : reader.GetString(7),
                    PANNumber = reader.IsDBNull(8) ? null : reader.GetString(8),
                    IsApproved = reader.GetBoolean(9),
                    ApprovedBy = reader.IsDBNull(10) ? (int?)null : reader.GetInt32(10),
                    CreatedBy = reader.GetInt32(11),
                    CreatedOn = reader.GetDateTime(12),
                    IsActive = reader.GetBoolean(13),
                });
            }

            return pendingVendors.Count > 0 ? Ok(pendingVendors) : NotFound();
        }
    }
}
