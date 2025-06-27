using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using vaulterpAPI.Models;

namespace vaulterpAPI.Controllers.Inventory
{
    [ApiController]
    [Route("api/inventory/[controller]")]
    public class RateCardController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public RateCardController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        private string GetConnectionString() =>
            _configuration.GetConnectionString("DefaultConnection");

        [HttpPost]
        public async Task<IActionResult> CreateRateCard([FromBody] RateCardDto dto)
        {
            if (dto == null || dto.ItemId <= 0 || dto.VendorId <= 0 || dto.Price <= 0 )
                return BadRequest("Invalid input data.");

            using var conn = new SqlConnection(GetConnectionString());
            var query = @"INSERT INTO Inventory.RateCard 
                            (ItemId, VendorId, Price, ValidTill, CreatedBy, IsApproved,IsActive) 
                          VALUES 
                            (@ItemId, @VendorId, @Price, @ValidTill, @CreatedBy, @IsApproved,1);
                          SELECT SCOPE_IDENTITY();";

            using var cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@ItemId", dto.ItemId);
            cmd.Parameters.AddWithValue("@VendorId", dto.VendorId);
            cmd.Parameters.AddWithValue("@Price", dto.Price);
            cmd.Parameters.AddWithValue("@ValidTill", (object?)dto.ValidTill ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@CreatedBy", 1);
            cmd.Parameters.AddWithValue("@IsApproved", 1);

            await conn.OpenAsync();
            var insertedId = Convert.ToInt32(await cmd.ExecuteScalarAsync());

            return Ok(new { message = "Rate card created successfully.", Id = insertedId });
        }

        [HttpGet]
        public async Task<IActionResult> GetAllRateCards([FromQuery] int? officeId = null,
                                                         [FromQuery] int? categoryId = null,
                                                         [FromQuery] int? itemId = null,
                                                         [FromQuery] int? vendorId = null)
        {
            using var conn = new SqlConnection(GetConnectionString());
            var query = @"
                SELECT rc.Id, 
                       cat.Name AS CategoryName,
                       i.Id AS ItemId,
                       i.Name AS ItemName,
                       i.BrandName,
                       i.Description,
                       i.HSNCode,
                       v.Id AS VendorId,
                       v.Name AS VendorName,
                       rc.Price,
                       i.MeasurementUnit,
                       rc.ValidTill,
                       rc.IsApproved,
                       rc.CreatedOn
                FROM Inventory.RateCard rc
                INNER JOIN Inventory.Item i ON rc.ItemId = i.Id
                INNER JOIN Inventory.Vendor v ON rc.VendorId = v.Id
                INNER JOIN Inventory.Category cat ON i.CategoryId = cat.Id
                WHERE rc.IsApproved = 1
                  AND (rc.IsActive > 0 AND (rc.ValidTill IS NULL OR rc.ValidTill >= GETDATE()))
                  AND i.IsApproved = 1 AND i.IsActive = 1
                  AND v.IsApproved = 1 AND v.IsActive = 1
                  AND cat.IsApproved = 1 AND cat.IsActive = 1
                  AND (@CategoryId IS NULL OR i.CategoryId = @CategoryId)
                  AND (@OfficeId IS NULL OR i.OfficeId = @OfficeId)
                  AND (@ItemId IS NULL OR rc.ItemId = @ItemId)
                  AND (@VendorId IS NULL OR rc.VendorId = @VendorId)
                ORDER BY rc.CreatedOn DESC";

            using var cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@OfficeId", (object?)officeId ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@CategoryId", (object?)categoryId ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@ItemId", (object?)itemId ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@VendorId", (object?)vendorId ?? DBNull.Value);

            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();
            var rateCards = new List<RateCardDto>();

            while (await reader.ReadAsync())
            {
                rateCards.Add(new RateCardDto
                {
                    Id = reader.GetInt32(0),
                    CategoryName = reader.GetString(1),
                    ItemId = reader.GetInt32(2),
                    ItemName = reader.GetString(3),
                    BrandName = reader.IsDBNull(4) ? null : reader.GetString(4),
                    Description = reader.IsDBNull(5) ? null : reader.GetString(5),
                    HSNCode = reader.IsDBNull(6) ? null : reader.GetString(6),
                    VendorId = reader.GetInt32(7),
                    VendorName = reader.GetString(8),
                    Price = reader.GetDecimal(9),
                    MeasurementUnit = reader.IsDBNull(10) ? null : reader.GetString(10),
                    ValidTill = reader.IsDBNull(11) ? (DateTime?)null : reader.GetDateTime(11),
                    IsApproved = reader.GetBoolean(12),
                    CreatedOn = reader.GetDateTime(13)
                });
            }

            return Ok(rateCards);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateRateCard(int id, [FromBody] RateCardDto dto)
        {
            if (dto == null) return BadRequest("Invalid rate card data.");
            using var conn = new SqlConnection(GetConnectionString());

            var query = @"UPDATE Inventory.RateCard
                          SET ItemId = @ItemId,
                              VendorId = @VendorId,
                              Price = @Price,
                              ValidTill = @ValidTill,
                              IsApproved = @IsApproved
                          WHERE Id = @Id";

            using var cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@Id", id);
            cmd.Parameters.AddWithValue("@ItemId", dto.ItemId);
            cmd.Parameters.AddWithValue("@VendorId", dto.VendorId);
            cmd.Parameters.AddWithValue("@Price", dto.Price);
            cmd.Parameters.AddWithValue("@ValidTill", (object?)dto.ValidTill ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@IsApproved", dto.IsApproved);

            await conn.OpenAsync();
            var rowsAffected = await cmd.ExecuteNonQueryAsync();

            return rowsAffected > 0
                ? Ok(new { message = "Rate card updated successfully." })
                : NotFound();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRateCard(int id)
        {
            using var conn = new SqlConnection(GetConnectionString());

            var query = @"UPDATE Inventory.RateCard 
                          SET IsActive = 0 
                          WHERE Id = @Id AND IsActive = 1";

            using var cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@Id", id);

            await conn.OpenAsync();
            var rowsAffected = await cmd.ExecuteNonQueryAsync();

            return rowsAffected > 0
                ? Ok(new { message = "Rate card deleted (soft) successfully" })
                : NotFound();
        }

        [HttpGet("pendingApproval")]
        public async Task<IActionResult> GetPendingApprovalRateCards()
        {
            using var conn = new SqlConnection(GetConnectionString());
            var query = @"
                SELECT rc.Id, 
                       cat.Name AS CategoryName,
                       i.Id AS ItemId,
                       i.Name AS ItemName,
                       v.Id AS VendorId,
                       v.Name AS VendorName,
                       rc.Price,
                       rc.ValidTill,
                       rc.IsApproved,
                       rc.CreatedOn
                FROM Inventory.RateCard rc
                INNER JOIN Inventory.Item i ON rc.ItemId = i.Id
                INNER JOIN Inventory.Vendor v ON rc.VendorId = v.Id
                INNER JOIN Inventory.Category cat ON i.CategoryId = cat.Id
                WHERE rc.IsApproved = 0 AND rc.IsActive = 1";

            using var cmd = new SqlCommand(query, conn);
            await conn.OpenAsync();

            using var reader = await cmd.ExecuteReaderAsync();
            var rateCards = new List<RateCardDto>();

            while (await reader.ReadAsync())
            {
                rateCards.Add(new RateCardDto
                {
                    Id = reader.GetInt32(0),
                    CategoryName = reader.GetString(1),
                    ItemId = reader.GetInt32(2),
                    ItemName = reader.GetString(3),
                    VendorId = reader.GetInt32(4),
                    VendorName = reader.GetString(5),
                    Price = reader.GetDecimal(6),
                    ValidTill = reader.IsDBNull(7) ? (DateTime?)null : reader.GetDateTime(7),
                    IsApproved = reader.GetBoolean(8),
                    CreatedOn = reader.GetDateTime(9),
                });
            }

            return Ok(rateCards);
        }
    }
}
