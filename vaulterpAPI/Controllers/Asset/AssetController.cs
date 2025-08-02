using Microsoft.AspNetCore.Mvc;
using Npgsql;
using vaulterpAPI.Models.Asset;

namespace vaulterpAPI.Controllers.Asset
{
    [ApiController]
    [Route("api/asset/[controller]")]
    public class AssetController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        public AssetController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        private string GetConnectionString() =>
            _configuration.GetConnectionString("DefaultConnection");

        [HttpGet]
        public async Task<IActionResult> GetAllAssets(int officeId)
        {
            var assets = new List<AssetDto>();
            using var conn = new NpgsqlConnection(GetConnectionString());
            using var cmd = new NpgsqlCommand(@"SELECT asset_id, asset_code, asset_name, asset_type_id, office_id, 
                                                       model_number, serial_number, purchase_date, warranty_expiry,
                                                       manufacturer, supplier, is_active, created_on, created_by
                                                FROM asset.asset_master
                                                WHERE office_id = @office_id AND is_active = true", conn);
            cmd.Parameters.AddWithValue("@office_id", officeId);
            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                assets.Add(new AssetDto
                {
                    AssetId = reader.GetInt32(0),
                    AssetCode = reader.GetString(1),
                    AssetName = reader.GetString(2),
                    AssetTypeId = reader.GetInt32(3),
                    OfficeId = reader.GetInt32(4),
                    ModelNumber = reader.IsDBNull(5) ? null : reader.GetString(5),
                    SerialNumber = reader.IsDBNull(6) ? null : reader.GetString(6),
                    PurchaseDate = reader.IsDBNull(7) ? null : reader.GetDateTime(7),
                    WarrantyExpiry = reader.IsDBNull(8) ? null : reader.GetDateTime(8),
                    Manufacturer = reader.IsDBNull(9) ? null : reader.GetString(9),
                    Supplier = reader.IsDBNull(10) ? null : reader.GetString(10),
                    IsActive = reader.GetBoolean(11),
                    CreatedOn = reader.IsDBNull(12) ? null : reader.GetDateTime(12),
                    CreatedBy = reader.IsDBNull(13) ? null : reader.GetString(13)
                });
            }
            return Ok(assets);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetAssetById(int id)
        {
            using var conn = new NpgsqlConnection(GetConnectionString());
            using var cmd = new NpgsqlCommand(@"SELECT asset_id, asset_code, asset_name, asset_type_id, office_id, 
                                                       model_number, serial_number, purchase_date, warranty_expiry,
                                                       manufacturer, supplier, is_active, created_on, created_by
                                                FROM asset.asset_master
                                                WHERE asset_id = @id", conn);
            cmd.Parameters.AddWithValue("@id", id);
            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                var asset = new AssetDto
                {
                    AssetId = reader.GetInt32(0),
                    AssetCode = reader.GetString(1),
                    AssetName = reader.GetString(2),
                    AssetTypeId = reader.GetInt32(3),
                    OfficeId = reader.GetInt32(4),
                    ModelNumber = reader.IsDBNull(5) ? null : reader.GetString(5),
                    SerialNumber = reader.IsDBNull(6) ? null : reader.GetString(6),
                    PurchaseDate = reader.IsDBNull(7) ? null : reader.GetDateTime(7),
                    WarrantyExpiry = reader.IsDBNull(8) ? null : reader.GetDateTime(8),
                    Manufacturer = reader.IsDBNull(9) ? null : reader.GetString(9),
                    Supplier = reader.IsDBNull(10) ? null : reader.GetString(10),
                    IsActive = reader.GetBoolean(11),
                    CreatedOn = reader.IsDBNull(12) ? null : reader.GetDateTime(12),
                    CreatedBy = reader.IsDBNull(13) ? null : reader.GetString(13)
                };
                return Ok(asset);
            }
            return NotFound();
        }

        [HttpPost]
        public async Task<IActionResult> CreateAsset([FromBody] AssetDto dto)
        {
            using var conn = new NpgsqlConnection(GetConnectionString());
            var query = @"INSERT INTO asset.asset_master (
                            asset_code, asset_name, asset_type_id, office_id,
                            model_number, serial_number, purchase_date, warranty_expiry,
                            manufacturer, supplier, is_active, created_on, created_by)
                          VALUES (
                            @asset_code, @asset_name, @asset_type_id, @office_id,
                            @model_number, @serial_number, @purchase_date, @warranty_expiry,
                            @manufacturer, @supplier, true, CURRENT_TIMESTAMP, @created_by)
                          RETURNING asset_id;";
            using var cmd = new NpgsqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@asset_code", dto.AssetCode);
            cmd.Parameters.AddWithValue("@asset_name", dto.AssetName);
            cmd.Parameters.AddWithValue("@asset_type_id", dto.AssetTypeId);
            cmd.Parameters.AddWithValue("@office_id", dto.OfficeId);
            cmd.Parameters.AddWithValue("@model_number", (object?)dto.ModelNumber ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@serial_number", (object?)dto.SerialNumber ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@purchase_date", (object?)dto.PurchaseDate ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@warranty_expiry", (object?)dto.WarrantyExpiry ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@manufacturer", (object?)dto.Manufacturer ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@supplier", (object?)dto.Supplier ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@created_by", (object?)dto.CreatedBy ?? DBNull.Value);
            await conn.OpenAsync();
            var newId = Convert.ToInt32(await cmd.ExecuteScalarAsync());
            return Ok(new { message = "Asset created successfully", id = newId });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAsset(int id, [FromBody] AssetDto dto)
        {
            using var conn = new NpgsqlConnection(GetConnectionString());
            var query = @"UPDATE asset.asset_master
                          SET asset_code = @asset_code,
                              asset_name = @asset_name,
                              asset_type_id = @asset_type_id,
                              office_id = @office_id,
                              model_number = @model_number,
                              serial_number = @serial_number,
                              purchase_date = @purchase_date,
                              warranty_expiry = @warranty_expiry,
                              manufacturer = @manufacturer,
                              supplier = @supplier,
                              is_active = @is_active,
                              created_by = @created_by
                          WHERE asset_id = @id";
            using var cmd = new NpgsqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@id", id);
            cmd.Parameters.AddWithValue("@asset_code", dto.AssetCode);
            cmd.Parameters.AddWithValue("@asset_name", dto.AssetName);
            cmd.Parameters.AddWithValue("@asset_type_id", dto.AssetTypeId);
            cmd.Parameters.AddWithValue("@office_id", dto.OfficeId);
            cmd.Parameters.AddWithValue("@model_number", (object?)dto.ModelNumber ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@serial_number", (object?)dto.SerialNumber ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@purchase_date", (object?)dto.PurchaseDate ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@warranty_expiry", (object?)dto.WarrantyExpiry ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@manufacturer", (object?)dto.Manufacturer ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@supplier", (object?)dto.Supplier ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@is_active", dto.IsActive);
            cmd.Parameters.AddWithValue("@created_by", (object?)dto.CreatedBy ?? DBNull.Value);
            await conn.OpenAsync();
            var rows = await cmd.ExecuteNonQueryAsync();
            return rows > 0 ? Ok("Asset updated successfully") : NotFound();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAsset(int id)
        {
            using var conn = new NpgsqlConnection(GetConnectionString());
            var query = @"UPDATE asset.asset_master SET is_active = false WHERE asset_id = @id";
            using var cmd = new NpgsqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@id", id);
            await conn.OpenAsync();
            var rows = await cmd.ExecuteNonQueryAsync();
            return rows > 0 ? Ok("Asset deactivated successfully") : NotFound();
        }
    }
}
