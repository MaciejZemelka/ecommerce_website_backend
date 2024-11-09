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
        private void DeleteExistingRefreshTokens(string userId)
        {
            using (var con = new SqlConnection(_configuration.GetConnectionString("ecommerce_DBcon")))
            {
                var cmd = new SqlCommand("DELETE FROM RefreshToken WHERE user_id = @UserId", con);
                cmd.Parameters.AddWithValue("@UserId", userId);

                con.Open();
                cmd.ExecuteNonQuery();
                con.Close();
            }
        }


        private void SaveRefreshTokenToDatabase(string userId, string RefreshToken)
        {
            DeleteExistingRefreshTokens(userId);

            SqlConnection con = new SqlConnection(_configuration.GetConnectionString("ecommerce_DBcon"));
            SqlCommand cmd = new SqlCommand("INSERT INTO RefreshToken(user_id,RefreshToken,ExpirationDate) VALUES (@userID, @Token, @ExpirationDate)", con);
            cmd.Parameters.AddWithValue("@UserId", userId);
            cmd.Parameters.AddWithValue("@Token", RefreshToken);
            cmd.Parameters.AddWithValue("@ExpirationDate", DateTime.UtcNow.AddDays(7));
            con.Open();
            cmd.ExecuteNonQuery();
            con.Close();
        }
        private string GetUserIdByRefreshToken(string refreshToken)
        {
            using (var con = new SqlConnection(_configuration.GetConnectionString("ecommerce_DBcon")))
            {
                var cmd = new SqlCommand("SELECT UserId FROM RefreshTokens WHERE Token = @Token AND ExpirationDate > @CurrentDate", con);
                cmd.Parameters.AddWithValue("@Token", refreshToken);
                cmd.Parameters.AddWithValue("@CurrentDate", DateTime.UtcNow);

                con.Open();
                var userId = cmd.ExecuteScalar() as string;
                return userId;
            }
        }

        [HttpPost]
        [Route("registration")]
        public IActionResult Registration(Registration registration)
        {
            SqlConnection con = new SqlConnection(_configuration.GetConnectionString("ecommerce_DBcon"));
            string HashedPass = PasswordEncryption.EncryptionedPassword(registration.Password);
            SqlCommand cmd = new SqlCommand("INSERT INTO users(Username,Email,Password,Permission, CreatedDate) VALUES('" + registration.Username + "','" + registration.Email + "','" + HashedPass + "','" + registration.Permission + "','" + registration.CreatedDate + "')", con);
            con.Open();
            int i = cmd.ExecuteNonQuery();
            con.Close();
            if (i > 0)
            {
                return Ok(new { message = "Data inserted" });
            }
            else
            {
                return Unauthorized("asd");
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
                        con.Close();
                        bool isPasswordValid = PasswordEncryption.VerifyPassword(login.Password, passwordHash);
                        
                        if (isPasswordValid)
                        {
                            var accessToken = _jwtService.GenerateAccessToken(userId);
                            var refreshToken = _jwtService.GenerateRefreshToken();

                            SaveRefreshTokenToDatabase(userId, refreshToken);

                            return Ok(new { AccessToken = accessToken, RefreshToken = refreshToken });
                        }
                        else
                        {
                            return Unauthorized("Wrong password");
                        }
                    }
                    else
                    {
                        return NotFound("No user");
                    }
                }
            }
        }
        [HttpPost]
        [Route("new-Access-token")]
        public IActionResult newAccessToken(string refreshToken)
        {
            // Sprawdź w bazie danych, czy refreshToken jest ważny i przypisany do użytkownika
            var userId = GetUserIdByRefreshToken(refreshToken);
            if (userId == null)
            {
                return Unauthorized("Invalid refresh token");
            }

            var newAccessToken = _jwtService.GenerateAccessToken(userId);
            


            return Ok(new { AccessToken = newAccessToken});
        }


    }
}
