using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Npgsql;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using vaulterpAPI.Models.Identity;

namespace vaulterpAPI.Controllers.Identity
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public AuthController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        private string GetConnectionString() =>
            _configuration.GetConnectionString("DefaultConnection");

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest login)
        {
            using var conn = new NpgsqlConnection(GetConnectionString());
            conn.Open();

            var cmd = new NpgsqlCommand(@"
    SELECT u.*, e.employee_name
    FROM identity.user u
    JOIN master.employee_master e ON u.employee_id = e.employee_id
    WHERE LOWER(u.email) = LOWER(@email) AND u.is_active = TRUE", conn);

            cmd.Parameters.AddWithValue("@email", login.Email);

            using var reader = cmd.ExecuteReader();
            if (!reader.Read())
            {
                return Unauthorized(new { message = "Invalid credentials" });
            }

            var storedHash = reader["password_hash"].ToString();

            bool passwordMatch = BCrypt.Net.BCrypt.Verify(login.Password, storedHash);
           
            if (!passwordMatch)
                return Unauthorized(new { message = "Invalid credentials" });

            var userId = (int)reader["id"];
            var userTypeId = (int)reader["usertype_id"];
            var employeeName = reader["employee_name"]?.ToString();


            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(ClaimTypes.Email, login.Email),
            new Claim(ClaimTypes.Role, userTypeId.ToString())
        }),
                Expires = DateTime.UtcNow.AddHours(4),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            var jwt = tokenHandler.WriteToken(token);

         
            return Ok(new
            {
                token = jwt,
                user = new
                {
                    id = userId,
                    email = login.Email,
                    usertypeId = userTypeId,
                    username = employeeName // 👈 Added only this

                }
            });
        }

    }


        public class LoginRequest
    {
        public string Email { get; set; } = "";
        public string Password { get; set; } = "";
    }
}
