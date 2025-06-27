using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using vaulterpAPI.Models;

namespace vaulterpAPI.Controllers.Inventory
{
    [ApiController]
    [Route("api/inventory/[controller]")]
    public class POController : ControllerBase
    {
        private readonly string _constr;

        public POController(IConfiguration configuration)
        {
            _constr = configuration.GetConnectionString("DefaultConnection");
        }

        [HttpGet("GetGroupedPurchaseOrderDetails")]
        public async Task<ActionResult<List<PODto>>> GetGroupedPurchaseOrderDetails([FromQuery] int officeId, [FromQuery] int? poId = null, [FromQuery] int? vendorId = null)
        {
            var flatList = new List<dynamic>();

            var query = @"
                SELECT 
                    PurchaseOrderId, PONumber, PODateTime, BillingAddress, ShippingAddress, OfficeId, IsApproved,
                    VendorId, VendorName, ContactPerson, ContactNumber, Email,
                    PurchaseOrderItemId, ItemId, ItemName, Quantity, Rate, LineTotal
                FROM Inventory.vw_PurchaseOrderDetails
                WHERE OfficeId = @OfficeId";

            if (poId.HasValue)
                query += " AND PurchaseOrderId = @POId";
            if (vendorId.HasValue)
                query += " AND VendorId = @VendorId";

            await using var conn = new SqlConnection(_constr);
            await conn.OpenAsync();

            await using var cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@OfficeId", officeId);
            if (poId.HasValue)
                cmd.Parameters.AddWithValue("@POId", poId.Value);
            if (vendorId.HasValue)
                cmd.Parameters.AddWithValue("@VendorId", vendorId.Value);

            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                flatList.Add(new
                {
                    PurchaseOrderId = reader.GetInt32(0),
                    PONumber = reader.GetString(1),
                    PODateTime = reader.GetDateTime(2),
                    BillingAddress = reader.GetString(3),
                    ShippingAddress = reader.GetString(4),
                    OfficeId = reader.GetInt32(5),
                    IsApproved = reader.GetBoolean(6),
                    VendorId = reader.GetInt32(7),
                    VendorName = reader.GetString(8),
                    ContactPerson = reader.GetString(9),
                    ContactNumber = reader.GetString(10),
                    Email = reader.GetString(11),
                    PurchaseOrderItemId = reader.GetInt32(12),
                    ItemId = reader.GetInt32(13),
                    ItemName = reader.GetString(14),
                    Quantity = Convert.ToDecimal(reader["Quantity"]),
                    Rate = Convert.ToDecimal(reader["Rate"]),
                    LineTotal = Convert.ToDecimal(reader["LineTotal"])
                });
            }

            var groupedResult = flatList
                .GroupBy(x => x.PurchaseOrderId)
                .Select(g => new PODto
                {
                    PurchaseOrderId = g.Key,
                    PONumber = g.First().PONumber,
                    PODateTime = g.First().PODateTime,
                    BillingAddress = g.First().BillingAddress,
                    ShippingAddress = g.First().ShippingAddress,
                    OfficeId = g.First().OfficeId,
                    IsApproved = g.First().IsApproved,
                    VendorId = g.First().VendorId,
                    VendorName = g.First().VendorName,
                    ContactPerson = g.First().ContactPerson,
                    ContactNumber = g.First().ContactNumber,
                    Email = g.First().Email,
                    Items = g.Select(item => new POItemDto
                    {
                        PurchaseOrderItemId = item.PurchaseOrderItemId,
                        ItemId = item.ItemId,
                        ItemName = item.ItemName,
                        Quantity = item.Quantity,
                        Rate = item.Rate,
                        LineTotal = item.LineTotal
                    }).ToList()
                })
                .ToList();

            return Ok(groupedResult);
        }

        [HttpPost("CreatePurchaseOrders")]
        public async Task<ActionResult<List<object>>> CreatePurchaseOrders([FromBody] List<CreatePurchaseOrderRequestDto> orders)
        {
            if (orders == null || !orders.Any())
                return BadRequest("No purchase orders provided.");

            await using var conn = new SqlConnection(_constr);
            await conn.OpenAsync();
            await using var transaction = conn.BeginTransaction();

            try
            {
                var createdOrders = new List<object>();

                foreach (var dto in orders)
                {
                    if (dto?.Items == null || !dto.Items.Any())
                        continue;

                    var tempPONumber = $"TEMP-{dto.OfficeId}-{DateTime.UtcNow:yyyyMMddHHmmssfff}";

                    var insertPOQuery = @"
                        INSERT INTO Inventory.PurchaseOrder 
                            (PONumber, VendorId, BillingAddress, ShippingAddress,TotalAmount, CreatedBy, OfficeId, IsApproved, IsDeleted)
                        VALUES 
                            (@PONumber, @VendorId, @BillingAddress, @ShippingAddress,@totalAmount, @CreatedBy, @OfficeId, 1, 0);
                        SELECT CAST(SCOPE_IDENTITY() AS int);";

                    int purchaseOrderId;
                    await using (var poCmd = new SqlCommand(insertPOQuery, conn, transaction))
                    {
                        poCmd.Parameters.AddWithValue("@PONumber", tempPONumber);
                        poCmd.Parameters.AddWithValue("@VendorId", dto.VendorId);
                        poCmd.Parameters.AddWithValue("@BillingAddress", (object?)dto.BillingAddress ?? DBNull.Value);
                        poCmd.Parameters.AddWithValue("@ShippingAddress", (object?)dto.ShippingAddress ?? DBNull.Value);
                        poCmd.Parameters.AddWithValue("@totalAmount", dto.TotalAmount);
                        poCmd.Parameters.AddWithValue("@CreatedBy", dto.CreatedBy);
                        poCmd.Parameters.AddWithValue("@OfficeId", dto.OfficeId);

                        purchaseOrderId = (int)await poCmd.ExecuteScalarAsync();
                    }

                    var poNumber = $"PO-{dto.OfficeId}{purchaseOrderId}";
                    var updatePOQuery = "UPDATE Inventory.PurchaseOrder SET PONumber = @PONumber WHERE Id = @Id";

                    await using (var updateCmd = new SqlCommand(updatePOQuery, conn, transaction))
                    {
                        updateCmd.Parameters.AddWithValue("@PONumber", poNumber);
                        updateCmd.Parameters.AddWithValue("@Id", purchaseOrderId);

                        await updateCmd.ExecuteNonQueryAsync();
                    }

                    foreach (var item in dto.Items)
                    {
                        var insertItemQuery = @"
                            INSERT INTO Inventory.PurchaseOrderItems 
                                (PurchaseOrderId, ItemId, Quantity, Rate, CreatedBy)
                            VALUES (@PurchaseOrderId, @ItemId, @Quantity, @Rate, @CreatedBy);";

                        await using (var itemCmd = new SqlCommand(insertItemQuery, conn, transaction))
                        {
                            itemCmd.Parameters.AddWithValue("@PurchaseOrderId", purchaseOrderId);
                            itemCmd.Parameters.AddWithValue("@ItemId", item.ItemId);
                            itemCmd.Parameters.AddWithValue("@Quantity", item.Quantity);
                            itemCmd.Parameters.AddWithValue("@Rate", item.Rate);
                            itemCmd.Parameters.AddWithValue("@CreatedBy", dto.CreatedBy);

                            await itemCmd.ExecuteNonQueryAsync();
                        }
                    }

                    createdOrders.Add(new
                    {
                        PurchaseOrderId = purchaseOrderId,
                        PONumber = poNumber,
                        Message = "Purchase Order created successfully."
                    });
                }

                await transaction.CommitAsync();
                return Ok(createdOrders);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, $"Error while creating Purchase Order: {ex.Message}");
            }
        }
    }
}
