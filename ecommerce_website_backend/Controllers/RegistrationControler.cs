using ecommerce_website_backend.Functions;
using ecommerce_website_backend.models;
using ecommerce_website_backend.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Security.AccessControl;




namespace ecommerce_website_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RegistrationController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly JwtService _jwtService;

        public RegistrationController(IConfiguration configuration, JwtService jwtService)
        {
            _configuration = configuration;
            _jwtService = jwtService;
        }

        [HttpPost]
        [Route("registration")]
        public string Registration(Registration registration)
        {
            SqlConnection con = new SqlConnection(_configuration.GetConnectionString("ecommerce_DBcon"));
            string HashedPass = PasswordEncryption.EncryptionedPassword(registration.Password);
            SqlCommand cmd = new SqlCommand("INSERT INTO users(Username,Email,Password,Permission, CreatedDate) VALUES('" + registration.Username + "','" + registration.Email + "','" + HashedPass + "','" + registration.Permission + "','" + registration.CreatedDate + "')", con);
            con.Open();
            int i = cmd.ExecuteNonQuery();
            con.Close();
            if (i > 0)
            {
                return "Data inserted";
            }
            else
            {
                return "Error";
            }
        }

        [HttpPost]
        [Route("login")]
        public IActionResult Login(Login login)
        {
            using (var con = new SqlConnection(_configuration.GetConnectionString("ecommerce_DBcon")))
            {
                SqlCommand cmd = new SqlCommand("SELECT Password, Id FROM Users WHERE Email = @Email", con);
                cmd.Parameters.AddWithValue("@Email", login.Email);

                con.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        string passwordHash = reader["Password"].ToString();
                        string userId = reader["Id"].ToString();

                        bool isPasswordValid = PasswordEncryption.VerifyPassword(login.Password, passwordHash);
                        if (isPasswordValid)
                        {
                            var accessToken = _jwtService.GenerateAccessToken(userId);
                            var refreshToken = _jwtService.GenerateRefreshToken();

                            // Opcjonalnie można zapisać refresh token w bazie danych dla użytkownika

                            return Ok(new { AccessToken = accessToken, RefreshToken = refreshToken });
                        }
                        else
                        {
                            return Unauthorized("Nieprawidłowe hasło");
                        }
                    }
                    else
                    {
                        return NotFound("Nie znaleziono użytkownika");
                    }
                }
            }
        }
    }
}
