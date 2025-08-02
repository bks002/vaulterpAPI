using Microsoft.AspNetCore.Mvc;
using Npgsql;
using vaulterpAPI.Models.Employee;
using vaulterpAPI.Models.Planning;

namespace vaulterpAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class JobCardController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public JobCardController(IConfiguration configuration)
        {
            _configuration = configuration;
        }


        [HttpPost("job_card")]//220725
        public IActionResult PostJobCard([FromBody] JobCard detail)
        {
            using (NpgsqlConnection conn = new NpgsqlConnection(GetConnectionString()))
            {
                string query = @"
    INSERT INTO Planning.job_card
    (order_no, is_code, date, asset_id, shift_id, operation_id, size, no_dia_of_stands,
     shape, is_compacted, compound, color, thickness, length, no_dia_of_am_wire,
     pay_off_dno, take_up_drum_size, embrossing, remark, created_by, created_on, updated_by, updated_on, office_id)
    VALUES
    (@order_no, @is_code, @date, @asset_id, @shift_id, @operation_id, @size, @no_dia_of_stands,
     @shape, @is_compacted, @compound, @color, @thickness, @length, @no_dia_of_am_wire,
     @pay_off_dno, @take_up_drum_size, @embrossing, @remark, @created_by, @created_on, @updated_by, @updated_on, @office_id)";

                using (NpgsqlCommand cmd = new NpgsqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@order_no", (object?)detail.OrderNo ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@is_code", (object?)detail.IsCode ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@date", detail.Date);
                    cmd.Parameters.AddWithValue("@asset_id", detail.AssetId);
                    cmd.Parameters.AddWithValue("@shift_id", detail.ShiftId);
                    cmd.Parameters.AddWithValue("@operation_id", detail.OperationId);
                    cmd.Parameters.AddWithValue("@size", (object?)detail.Size ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@no_dia_of_stands", (object?)detail.NoDiaOfStands ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@shape", (object?)detail.Shape ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@is_compacted", detail.IsCompacted);
                    cmd.Parameters.AddWithValue("@compound", (object?)detail.Compound ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@color", (object?)detail.Color ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@thickness", (object?)detail.Thickness ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@length", (object?)detail.Length ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@no_dia_of_am_wire", (object?)detail.NoDiaOfAmWire ?? DBNull.Value);

                    cmd.Parameters.AddWithValue("@pay_off_dno", (object?)detail.PayOffDno ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@take_up_drum_size", (object?)detail.TakeUpDrumSize ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@embrossing", (object?)detail.Embrossing ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@remark", (object?)detail.Remark ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@created_by", detail.CreatedBy);
                    cmd.Parameters.AddWithValue("@created_on", detail.CreatedOn);
                    cmd.Parameters.AddWithValue("@updated_by", (object?)detail.UpdatedBy ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@updated_on", (object?)detail.UpdatedOn ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@office_id", detail.OfficeId);


                    conn.Open();
                    int rowsAffected = cmd.ExecuteNonQuery();

                    return Ok(new { message = $"{rowsAffected} record inserted successfully." });
                }
            }
        }


        [HttpGet("job_card")]
        public IActionResult GetJobCards([FromQuery] int? officeId = null)
        {
            List<JobCard> jobCards = new List<JobCard>();

            using (NpgsqlConnection conn = new NpgsqlConnection(GetConnectionString()))
            {
                string query = @"SELECT id, order_no, is_code, date, asset_id, shift_id, operation_id, size, 
                                no_dia_of_stands, shape, is_compacted, compound, color, thickness, length, 
                                no_dia_of_am_wire, pay_off_dno, take_up_drum_size, embrossing, remark, 
                                created_by, created_on, updated_by, updated_on, office_id
                         FROM Planning.job_card
                         WHERE is_deleted = false " +
                                 (officeId.HasValue ? " AND office_id = @OfficeId" : "");

                using (NpgsqlCommand cmd = new NpgsqlCommand(query, conn))
                {
                    if (officeId.HasValue)
                        cmd.Parameters.AddWithValue("@OfficeId", officeId.Value);

                    conn.Open();
                    using (NpgsqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            JobCard jobCard = new JobCard
                            {
                                Id = reader.GetInt32(0),
                                OrderNo = reader["order_no"]?.ToString(),
                                IsCode = reader["is_code"]?.ToString(),
                                Date = reader.GetDateTime(reader.GetOrdinal("date")),
                                AssetId = reader.GetInt32(reader.GetOrdinal("asset_id")),
                                ShiftId = reader.GetInt32(reader.GetOrdinal("shift_id")),
                                OperationId = reader.GetInt32(reader.GetOrdinal("operation_id")),
                                Size = reader["size"] != DBNull.Value ? Convert.ToInt32(reader["size"]) : null,
                                NoDiaOfStands = reader["no_dia_of_stands"]?.ToString(),
                                Shape = reader["shape"]?.ToString(),
                                IsCompacted = reader.GetBoolean(reader.GetOrdinal("is_compacted")),
                                Compound = reader["compound"]?.ToString(),
                                Color = reader["color"]?.ToString(),
                                Thickness = reader["thickness"]?.ToString(),
                                Length = reader["length"]?.ToString(),
                                NoDiaOfAmWire = reader["no_dia_of_am_wire"]?.ToString(),
                                PayOffDno = reader["pay_off_dno"]?.ToString(),
                                TakeUpDrumSize = reader["take_up_drum_size"]?.ToString(),
                                Embrossing = reader["embrossing"]?.ToString(),
                                Remark = reader["remark"]?.ToString(),
                                CreatedBy = reader.GetInt32(reader.GetOrdinal("created_by")),
                                CreatedOn = reader.GetDateTime(reader.GetOrdinal("created_on")),
                                UpdatedBy = reader["updated_by"] != DBNull.Value ? Convert.ToInt32(reader["updated_by"]) : null,
                                UpdatedOn = reader["updated_on"] != DBNull.Value ? Convert.ToDateTime(reader["updated_on"]) : null,
                                OfficeId = reader.GetInt32(reader.GetOrdinal("office_id")),
                            };

                            jobCards.Add(jobCard);
                        }
                    }
                }
            }

            return Ok(jobCards);
        }

        private string GetConnectionString() =>
            _configuration.GetConnectionString("DefaultConnection");

        [HttpPut("job_card/{id}")] //220725
        public IActionResult UpdateJobCard(int id, [FromBody] JobCard jobCard)
        {
            if (id != jobCard.Id)
                return BadRequest("ID mismatch.");

            using (NpgsqlConnection conn = new NpgsqlConnection(GetConnectionString()))
            {
                string query = @"
UPDATE Planning.job_card
SET order_no = @OrderNo,
    is_code = @IsCode,
    date = @Date,
    asset_id = @AssetId,
    shift_id = @ShiftId,
    operation_id = @OperationId,
    size = @Size,
    no_dia_of_stands = @NoDiaOfStands,
    shape = @Shape,
    is_compacted = @IsCompacted,
    compound = @Compound,
    color = @Color,
    thickness = @Thickness,
    length = @Length,
    no_dia_of_am_wire = @NoDiaOfAmWire,
    pay_off_dno = @PayOffDno,
    take_up_drum_size = @TakeUpDrumSize,
    embrossing = @Embrossing,
    remark = @Remark,
    updated_by = @UpdatedBy,
    updated_on = @UpdatedOn,
    office_id = @OfficeId
WHERE id = @Id";

                using (NpgsqlCommand cmd = new NpgsqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    cmd.Parameters.AddWithValue("@OrderNo", jobCard.OrderNo ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@IsCode", jobCard.IsCode ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Date", jobCard.Date);
                    cmd.Parameters.AddWithValue("@AssetId", jobCard.AssetId);
                    cmd.Parameters.AddWithValue("@ShiftId", jobCard.ShiftId);
                    cmd.Parameters.AddWithValue("@OperationId", jobCard.OperationId);
                    cmd.Parameters.AddWithValue("@Size", jobCard.Size ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@NoDiaOfStands", jobCard.NoDiaOfStands ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Shape", jobCard.Shape ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@IsCompacted", jobCard.IsCompacted);
                    cmd.Parameters.AddWithValue("@Compound", jobCard.Compound ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Color", jobCard.Color ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Thickness", jobCard.Thickness ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Length", jobCard.Length ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@NoDiaOfAmWire", jobCard.NoDiaOfAmWire ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@PayOffDno", jobCard.PayOffDno ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@TakeUpDrumSize", jobCard.TakeUpDrumSize ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Embrossing", jobCard.Embrossing ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Remark", jobCard.Remark ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@UpdatedBy", jobCard.UpdatedBy ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@UpdatedOn", jobCard.UpdatedOn ?? DateTime.Now);
                    cmd.Parameters.AddWithValue("@OfficeId", jobCard.OfficeId);

                    conn.Open();
                    int rowsAffected = cmd.ExecuteNonQuery();

                    if (rowsAffected > 0)
                        return Ok("JobCard updated successfully.");
                    else
                        return NotFound("JobCard not found.");
                }
            }
        }


        [HttpDelete("job_card/{id}")]//220725
        public IActionResult DeleteJobCard(int id)
        {
            using (NpgsqlConnection conn = new NpgsqlConnection(GetConnectionString()))
            {
                string query = @"UPDATE Planning.job_card 
                 SET is_deleted = true, updated_on = CURRENT_TIMESTAMP 
                 WHERE id = @Id";

                using (NpgsqlCommand cmd = new NpgsqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);

                    conn.Open();
                    int rowsAffected = cmd.ExecuteNonQuery();
                    conn.Close();

                    if (rowsAffected == 0)
                        return NotFound("No record found with the given ID.");

                    return Ok($"Record with ID {id} marked as true.");
                }
            }
        }




    }
}
