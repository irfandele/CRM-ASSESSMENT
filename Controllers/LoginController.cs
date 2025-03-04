using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Text.Json;

namespace CRM.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class LoginController : ControllerBase
    {
        private readonly string _connectionString;

        public LoginController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection") ?? throw new ArgumentNullException(nameof(configuration));
        }

        [HttpPost("Authenticate")]
        public IActionResult Authenticate([FromBody] LoginRequest request)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    var cmd = new SqlCommand(
                        "SELECT EmpID, Role FROM Users WHERE Username = @Username AND PasswordHash = @Password",
                        connection);
                    cmd.Parameters.AddWithValue("@Username", request.Username);
                    cmd.Parameters.AddWithValue("@Password", request.Password); // Replace with hashing if needed

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            var empId = reader.GetGuid(0);
                            var role = reader.GetString(1);
                            HttpContext.Session.SetString("EmpID", empId.ToString());
                            return Ok(new { role }); // Return JSON with role
                        }
                    }
                }
                return Unauthorized(new { error = "Invalid credentials" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Login Error: {ex.Message}");
                return StatusCode(500, new { error = "Server error during login" });
            }
        }
    }

    public class LoginRequest
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }
}