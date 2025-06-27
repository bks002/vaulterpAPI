using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using vaulterpAPI.Models.Inventory;

namespace vaulterpAPI.Controllers.Inventory
{
    [ApiController]
    [Route("api/inventory/[controller]")]
    public class CategoryController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public CategoryController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        private string GetConnectionString() =>
            _configuration.GetConnectionString("DefaultConnection");

        [HttpGet]
        public async Task<IActionResult> GetAllCategories(int officeId)
        {
            var categories = new List<CategoryDto>();

            using var conn = new SqlConnection(GetConnectionString());
            using var cmd = new SqlCommand(@"SELECT Id, OfficeId, Name, Description, IsActive, IsApproved, CreatedOn, CreatedBy 
                                              FROM Inventory.Category 
                                              WHERE OfficeId = @OfficeId AND IsActive = 1 AND IsApproved = 1", conn);
            cmd.Parameters.AddWithValue("@OfficeId", officeId);
            await conn.OpenAsync();

            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                categories.Add(new CategoryDto
                {
                    Id = reader.GetInt32(0),
                    OfficeId = reader.GetInt32(1),
                    Name = reader.GetString(2),
                    Description = reader.IsDBNull(3) ? null : reader.GetString(3),
                    IsActive = reader.GetBoolean(4),
                    IsApproved = reader.GetBoolean(5),
                    CreatedOn = reader.GetDateTime(6),
                    CreatedBy = reader.GetInt32(7)
                });
            }

            return categories.Count > 0 ? Ok(categories) : NotFound();
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetCategoryById(int id)
        {
            using var conn = new SqlConnection(GetConnectionString());
            using var cmd = new SqlCommand(@"SELECT Id, OfficeId, Name, Description, IsActive, IsApproved, CreatedOn, CreatedBy 
                                              FROM Inventory.Category 
                                              WHERE Id = @Id", conn);
            cmd.Parameters.AddWithValue("@Id", id);

            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                var category = new CategoryDto
                {
                    Id = reader.GetInt32(0),
                    OfficeId = reader.GetInt32(1),
                    Name = reader.GetString(2),
                    Description = reader.IsDBNull(3) ? null : reader.GetString(3),
                    IsActive = reader.GetBoolean(4),
                    IsApproved = reader.GetBoolean(5),
                    CreatedOn = reader.GetDateTime(6),
                    CreatedBy = reader.GetInt32(7),
                };
                return Ok(category);
            }

            return NotFound();
        }

        [HttpPost]
        public async Task<IActionResult> CreateCategory([FromBody] CategoryDto dto)
        {
            if (dto == null) return BadRequest("Invalid category data.");
            using var conn = new SqlConnection(GetConnectionString());

            var query = @"INSERT INTO Inventory.Category (OfficeId, Name, Description, CreatedBy, CreatedOn, IsActive, IsApproved)
                          VALUES (@OfficeId, @Name, @Description, @CreatedBy, GETDATE(), 1, 1);
                          SELECT SCOPE_IDENTITY();";

            using var cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@OfficeId", dto.OfficeId);
            cmd.Parameters.AddWithValue("@Name", dto.Name);
            cmd.Parameters.AddWithValue("@Description", (object?)dto.Description ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@CreatedBy", dto.CreatedBy);

            await conn.OpenAsync();
            var insertedId = Convert.ToInt32(await cmd.ExecuteScalarAsync());

            return Ok(new { message = "Category created successfully", Id = insertedId });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCategory(int id, [FromBody] CategoryDto dto)
        {
            if (dto == null) return BadRequest("Invalid category data.");
            using var conn = new SqlConnection(GetConnectionString());

            var query = @"UPDATE Inventory.Category
                          SET Name = @Name, Description = @Description, IsActive = @IsActive,
                              IsApproved = @IsApproved, ApprovedBy = @ApprovedBy
                          WHERE Id = @Id";

            using var cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@Id", id);
            cmd.Parameters.AddWithValue("@Name", dto.Name);
            cmd.Parameters.AddWithValue("@Description", (object?)dto.Description ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@IsActive", dto.IsActive);
            cmd.Parameters.AddWithValue("@IsApproved", dto.IsApproved);
            cmd.Parameters.AddWithValue("@ApprovedBy", (object?)dto.ApprovedBy ?? DBNull.Value);

            await conn.OpenAsync();
            var rowsAffected = await cmd.ExecuteNonQueryAsync();

            return rowsAffected > 0
                ? Ok(new { message = "Category updated successfully" })
                : NotFound();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            using var conn = new SqlConnection(GetConnectionString());

            var query = @"UPDATE Inventory.Category 
                          SET IsActive = 0 
                          WHERE Id = @Id AND IsActive = 1";

            using var cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@Id", id);

            await conn.OpenAsync();
            var rowsAffected = await cmd.ExecuteNonQueryAsync();

            return rowsAffected > 0
                ? Ok(new { message = "Category deleted (soft) successfully" })
                : NotFound();
        }

        [HttpGet("pendingApproval")]
        public async Task<IActionResult> GetPendingApprovalCategories(int officeId)
        {
            var pendingCategories = new List<CategoryDto>();

            using var conn = new SqlConnection(GetConnectionString());
            using var cmd = new SqlCommand(@"SELECT Id, OfficeId, Name, Description, IsActive, IsApproved, CreatedOn, CreatedBy
                                              FROM Inventory.Category
                                              WHERE OfficeId = @OfficeId AND IsActive = 1 AND IsApproved = 0", conn);
            cmd.Parameters.AddWithValue("@OfficeId", officeId);

            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                pendingCategories.Add(new CategoryDto
                {
                    Id = reader.GetInt32(0),
                    OfficeId = reader.GetInt32(1),
                    Name = reader.GetString(2),
                    Description = reader.IsDBNull(3) ? null : reader.GetString(3),
                    IsActive = reader.GetBoolean(4),
                    IsApproved = reader.GetBoolean(5),
                    CreatedOn = reader.GetDateTime(6),
                    CreatedBy = reader.GetInt32(7),
                });
            }

            return Ok(pendingCategories);
        }
    }
}
