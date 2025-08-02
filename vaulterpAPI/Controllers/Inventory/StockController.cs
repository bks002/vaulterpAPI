using Microsoft.AspNetCore.Mvc;
using Npgsql;
using vaulterpAPI.Models.Inventory;


namespace YourNamespace.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StockController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public StockController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet("office")]
        public async Task<IActionResult> GetStockByOfficeId([FromQuery] int office_id)
        {
            var stockList = new List<StockDto>();
            string connectionString = _configuration.GetConnectionString("DefaultConnection");

            await using var conn = new NpgsqlConnection(connectionString);
            await conn.OpenAsync();

            string query = @"SELECT 
                        s.stock_id, 
                        s.item_id, 
                        s.office_id, 
                        s.current_qty, 
                        s.min_qty,
                        i.name,
                        i.description,
                        i.category_id
                    FROM 
                        inventory.stock s
                    INNER JOIN 
                        inventory.item i ON s.item_id = i.id
                    WHERE 
                        s.office_id = @office_id";

            await using var cmd = new NpgsqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@office_id", office_id);

            await using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                stockList.Add(new StockDto
                {
                    stock_id = reader.GetInt32(0),
                    item_id = reader.GetInt32(1),
                    office_id = reader.GetInt32(2),
                    current_qty = reader.GetInt32(3),
                    min_qty = reader.GetInt32(4),
                    name = reader.GetString(5),            
                    description = reader.GetString(6),
                    category_id = reader.GetInt32(7)
                });
            }

            return Ok(stockList);
        }
    }
}
