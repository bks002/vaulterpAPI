using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using vaulterpAPI.Models.Inventory;

namespace vaulterpAPI.Controllers.Inventory
{
    [ApiController]
    [Route("api/inventory/[controller]")]
    public class ItemController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public ItemController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        private string GetConnectionString() =>
            _configuration.GetConnectionString("DefaultConnection");

        [HttpGet]
        public async Task<IActionResult> GetAllItems(int officeId, int? categoryId = null)
        {
            if (officeId <= 0) return BadRequest("OfficeId is required.");
            var items = new List<ItemDto>();

            using var conn = new SqlConnection(GetConnectionString());
            var query = @"SELECT Id, OfficeId, CategoryId, Name, Description, MeasurementUnit, MinStockLevel,
                                 IsActive, IsApproved, ApprovedBy, CreatedOn, CreatedBy, BrandName, HSNCode
                          FROM Inventory.Item
                          WHERE OfficeId = @OfficeId AND IsActive = 1 AND IsApproved = 1";

            if (categoryId.HasValue) query += " AND CategoryId = @CategoryId";

            using var cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@OfficeId", officeId);
            if (categoryId.HasValue) cmd.Parameters.AddWithValue("@CategoryId", categoryId.Value);

            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                items.Add(new ItemDto
                {
                    Id = reader.GetInt32(0),
                    OfficeId = reader.GetInt32(1),
                    CategoryId = reader.GetInt32(2),
                    Name = reader.GetString(3),
                    Description = reader.IsDBNull(4) ? null : reader.GetString(4),
                    MeasurementUnit = reader.IsDBNull(5) ? null : reader.GetString(5),
                    MinStockLevel = reader.GetInt32(6),
                    IsActive = reader.GetBoolean(7),
                    IsApproved = reader.GetBoolean(8),
                    ApprovedBy = reader.IsDBNull(9) ? (int?)null : reader.GetInt32(9),
                    CreatedOn = reader.GetDateTime(10),
                    CreatedBy = reader.GetInt32(11),
                    BrandName = reader.IsDBNull(12) ? null : reader.GetString(12),
                    HSNCode = reader.IsDBNull(13) ? null : reader.GetString(13)
                });
            }

            return Ok(items);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetItemById(int id)
        {
            using var conn = new SqlConnection(GetConnectionString());
            using var cmd = new SqlCommand(@"SELECT Id, OfficeId, CategoryId, Name, Description, MeasurementUnit, MinStockLevel,
                                                  IsActive, IsApproved, ApprovedBy, CreatedOn, CreatedBy, BrandName, HSNCode
                                           FROM Inventory.Item
                                           WHERE Id = @Id", conn);
            cmd.Parameters.AddWithValue("@Id", id);

            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                var item = new ItemDto
                {
                    Id = reader.GetInt32(0),
                    OfficeId = reader.GetInt32(1),
                    CategoryId = reader.GetInt32(2),
                    Name = reader.GetString(3),
                    Description = reader.IsDBNull(4) ? null : reader.GetString(4),
                    MeasurementUnit = reader.IsDBNull(5) ? null : reader.GetString(5),
                    MinStockLevel = reader.GetInt32(6),
                    IsActive = reader.GetBoolean(7),
                    IsApproved = reader.GetBoolean(8),
                    ApprovedBy = reader.IsDBNull(9) ? (int?)null : reader.GetInt32(9),
                    CreatedOn = reader.GetDateTime(10),
                    CreatedBy = reader.GetInt32(11),
                    BrandName = reader.IsDBNull(12) ? null : reader.GetString(12),
                    HSNCode = reader.IsDBNull(13) ? null : reader.GetString(13)
                };
                return Ok(item);
            }

            return NotFound();
        }

        [HttpPost]
        public async Task<IActionResult> CreateItem([FromBody] ItemDto dto)
        {
            if (dto == null) return BadRequest("Invalid item data.");
            using var conn = new SqlConnection(GetConnectionString());

            var query = @"INSERT INTO Inventory.Item 
                          (OfficeId, CategoryId, Name, Description, MeasurementUnit, MinStockLevel, CreatedBy, CreatedOn, IsActive, IsApproved, BrandName, HSNCode) 
                          VALUES (@OfficeId, @CategoryId, @Name, @Description, @MeasurementUnit, @MinStockLevel, @CreatedBy, GETDATE(), 1,1, @BrandName, @HSNCode);
                          SELECT SCOPE_IDENTITY();";

            using var cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@OfficeId", dto.OfficeId);
            cmd.Parameters.AddWithValue("@CategoryId", dto.CategoryId);
            cmd.Parameters.AddWithValue("@Name", dto.Name);
            cmd.Parameters.AddWithValue("@Description", (object?)dto.Description ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@MeasurementUnit", (object?)dto.MeasurementUnit ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@MinStockLevel", dto.MinStockLevel);
            cmd.Parameters.AddWithValue("@CreatedBy", dto.CreatedBy);
            cmd.Parameters.AddWithValue("@BrandName", (object?)dto.BrandName ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@HSNCode", (object?)dto.HSNCode ?? DBNull.Value);

            await conn.OpenAsync();
            var insertedId = Convert.ToInt32(await cmd.ExecuteScalarAsync());

            return Ok(new { message = "Item created successfully", Id = insertedId });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateItem(int id, [FromBody] ItemDto dto)
        {
            if (dto == null) return BadRequest("Invalid item data.");
            using var conn = new SqlConnection(GetConnectionString());

            var query = @"UPDATE Inventory.Item 
                          SET Name = @Name,
                              Description = @Description,
                              CategoryId = @CategoryId,
                              MeasurementUnit = @MeasurementUnit,
                              MinStockLevel = @MinStockLevel,
                              IsActive = @IsActive,
                              IsApproved = @IsApproved,
                              ApprovedBy = @ApprovedBy,
                              BrandName = @BrandName,
                              HSNCode = @HSNCode
                          WHERE Id = @Id";

            using var cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@Id", id);
            cmd.Parameters.AddWithValue("@Name", dto.Name);
            cmd.Parameters.AddWithValue("@Description", (object?)dto.Description ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@CategoryId", dto.CategoryId);
            cmd.Parameters.AddWithValue("@MeasurementUnit", (object?)dto.MeasurementUnit ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@MinStockLevel", dto.MinStockLevel);
            cmd.Parameters.AddWithValue("@IsActive", dto.IsActive);
            cmd.Parameters.AddWithValue("@IsApproved", dto.IsApproved);
            cmd.Parameters.AddWithValue("@ApprovedBy", (object?)dto.ApprovedBy ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@BrandName", (object?)dto.BrandName ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@HSNCode", (object?)dto.HSNCode ?? DBNull.Value);

            await conn.OpenAsync();
            var rowsAffected = await cmd.ExecuteNonQueryAsync();

            return rowsAffected > 0 ? Ok(new { message = "Item updated successfully" }) : NotFound();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteItem(int id)
        {
            using var conn = new SqlConnection(GetConnectionString());

            var query = @"UPDATE Inventory.Item
                          SET IsActive = 0
                          WHERE Id = @Id AND IsActive = 1";

            using var cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@Id", id);

            await conn.OpenAsync();
            var rowsAffected = await cmd.ExecuteNonQueryAsync();

            return rowsAffected > 0
                ? Ok(new { message = "Item deleted (soft) successfully" })
                : NotFound();
        }

        [HttpGet("pendingApproval")]
        public async Task<IActionResult> GetUnapprovedItems(int officeId)
        {
            if (officeId <= 0) return BadRequest("OfficeId is required.");
            var items = new List<ItemDto>();

            using var conn = new SqlConnection(GetConnectionString());
            var query = @"SELECT Id, OfficeId, CategoryId, Name, Description, MeasurementUnit, MinStockLevel,
                                 IsActive, IsApproved, ApprovedBy, CreatedOn, CreatedBy, BrandName, HSNCode
                          FROM Inventory.Item
                          WHERE OfficeId = @OfficeId AND IsActive = 1 AND IsApproved = 0";

            using var cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@OfficeId", officeId);

            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                items.Add(new ItemDto
                {
                    Id = reader.GetInt32(0),
                    OfficeId = reader.GetInt32(1),
                    CategoryId = reader.GetInt32(2),
                    Name = reader.GetString(3),
                    Description = reader.IsDBNull(4) ? null : reader.GetString(4),
                    MeasurementUnit = reader.IsDBNull(5) ? null : reader.GetString(5),
                    MinStockLevel = reader.GetInt32(6),
                    IsActive = reader.GetBoolean(7),
                    IsApproved = reader.GetBoolean(8),
                    ApprovedBy = reader.IsDBNull(9) ? (int?)null : reader.GetInt32(9),
                    CreatedOn = reader.GetDateTime(10),
                    CreatedBy = reader.GetInt32(11),
                    BrandName = reader.IsDBNull(12) ? null : reader.GetString(12),
                    HSNCode = reader.IsDBNull(13) ? null : reader.GetString(13)
                });
            }

            return Ok(items);
        }
    }
}
