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
using System.Runtime.InteropServices;


namespace ecommerce_website_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly JwtService _jwtService;

        public UserController(IConfiguration configuration, JwtService jwtService)
        {
            _configuration = configuration;
            _jwtService = jwtService;
        }
        private string GetUserIdByRefreshToken(string refreshToken)
        {
            using (SqlConnection con = new SqlConnection(_configuration.GetConnectionString("ecommerce_DBcon")))
            {
                SqlCommand cmd = new SqlCommand("SELECT user_id FROM RefreshToken WHERE RefreshToken = @RefreshToken AND ExpirationDate > @CurrentDate ", con);
                cmd.Parameters.AddWithValue("@RefreshToken", refreshToken);

                cmd.Parameters.AddWithValue("@CurrentDate", DateTime.UtcNow.Date);
                con.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        string userId = reader["user_id"].ToString();

                        con.Close();
                        return userId;
                    }
                    else
                    {
                        return "";
                    }
                }
            }
        }

        [HttpPost]
        [Route("UserDetails")]
        public IActionResult UserDetails(GetRefreshToken refreshToken)
        {
            string userId = GetUserIdByRefreshToken(refreshToken.RefreshToken);

            using (var con = new SqlConnection(_configuration.GetConnectionString("ecommerce_DBcon")))
            {
                SqlCommand cmd = new SqlCommand("SELECT " +
                    " u.Email, u.CreatedDate, ud.first_name, ud.last_name, ud.PhoneNumber, ud.date_of_birth, ud.gender, ua.Country, ua.City, ua.StreetName, ua.HouseNumber, ua.ApartmentNumber, ua.PostalCode" +
                    " FROM Users u" +
                    " LEFT JOIN UserDetails ud ON u.ID = ud.user_id " +
                    " LEFT JOIN UserAddresses ua ON u.ID = ua.user_id " +
                    " WHERE u.ID = @userId ", con);
                cmd.Parameters.AddWithValue("@userId", userId);

                con.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        string email = reader["Email"].ToString();
                        string createdDate = reader["CreatedDate"].ToString();
                        string firstName = reader["first_name"]?.ToString();
                        string lastName = reader["last_name"]?.ToString();
                        string phoneNumber = reader["PhoneNumber"]?.ToString();
                        string dateOfBirth = reader["date_of_birth"]?.ToString();
                        string gender = reader["gender"]?.ToString();
                        string country = reader["Country"]?.ToString();
                        string city = reader["City"]?.ToString();
                        string streetName = reader["StreetName"]?.ToString();
                        string houseNumber = reader["HouseNumber"]?.ToString();
                        string apartmentNumber = reader["ApartmentNumber"]?.ToString();
                        string postalCode = reader["PostalCode"]?.ToString();

                        con.Close();
                        return Ok(new
                        {
                            email = email,
                            createdDate = createdDate,
                            firstName = firstName,
                            lastName = lastName,
                            phoneNumber = phoneNumber,
                            dateOfBirth = dateOfBirth,
                            gender = gender,
                            country = country,
                            city = city,
                            streetName = streetName,
                            houseNumber = houseNumber,
                            apartmentNumber = apartmentNumber,
                            postalCode = postalCode

                        });
                    }
                    return NotFound("user not found");
                }
            }
        }
    }

}
