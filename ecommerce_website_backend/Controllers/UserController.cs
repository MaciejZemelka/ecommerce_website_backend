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
using System.Collections.Immutable;
using Microsoft.AspNetCore.SignalR.Protocol;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System.Reflection.Metadata.Ecma335;


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
                    " u.Email, u.CreatedDate, ud.first_name, ud.last_name, ud.PhoneNumber, ud.date_of_birth, ud.gender" +
                    " FROM Users u" +
                    " LEFT JOIN UserDetails ud ON u.ID = ud.user_id " +
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
                        });
                    }
                    return NotFound("user not found");
                }
            }
        }

        [HttpPost]
        [Route("UserAddresses")]
        public IActionResult UserAddresses(GetRefreshToken refreshToken)
        {
            string userId = GetUserIdByRefreshToken(refreshToken.RefreshToken);

            using (var con = new SqlConnection(_configuration.GetConnectionString("ecommerce_DBcon")))
            {
                SqlCommand cmd = new SqlCommand("SELECT " +
                    " ua.address_id, ua.Country, ua.City, ua.StreetName, ua.HouseNumber, ua.ApartmentNumber, ua.PostalCode" +
                    " FROM Users u" +
                    " LEFT JOIN UserAddresses ua ON u.ID = ua.user_id " +
                    " WHERE u.ID = @userId ", con);
                cmd.Parameters.AddWithValue("@userId", userId);

                con.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    var addresses = new List<object>();
                    bool userExists = false;
                    bool hasAddress = false;

                    while (reader.Read())
                    {
                        if (!userExists)
                        {
                            userExists = true;
                        }

                        hasAddress = true;

                        if (reader["address_id"]?.ToString() == "")
                        {
                            con.Close();
                            return NotFound("User has no addresses");
                        }

                        var address = new
                        {

                            id = reader["address_id"]?.ToString(),
                            Country = reader["Country"]?.ToString(),
                            City = reader["City"]?.ToString(),
                            StreetName = reader["StreetName"]?.ToString(),
                            HouseNumber = reader["HouseNumber"]?.ToString(),
                            ApartmentNumber = reader["ApartmentNumber"]?.ToString(),
                            PostalCode = reader["PostalCode"]?.ToString()
                        };



                        addresses.Add(address);


                    }

                    con.Close();

                    if (addresses.Count > 0)
                    {

                        return Ok(new { Addresses = addresses });
                    }

                    return NotFound("User does not exist." + addresses.Count);
                }

            }
        }

        [HttpPost]
        [Route("AddNewAddress")]
        [Authorize]
        public IActionResult AddNewAddress(NewAddress newAddress)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return Unauthorized("Token is invalid or missing.");
            }

            if (int.TryParse(userIdClaim.Value, out int userId)) ;
               
         
            
            
            SqlConnection con = new SqlConnection(_configuration.GetConnectionString("ecommerce_DBcon"));
            SqlCommand cmd = new SqlCommand("INSERT INTO UserAddresses(user_id, Country, City, StreetName, HouseNumber, ApartmentNumber, PostalCode) Values(@userId,@Country,@City,@StreetName,@HouseNumber,@ApartmentNumber,@PostalCode)", con);
            cmd.Parameters.AddWithValue("@UserId", userId);
            cmd.Parameters.AddWithValue("@Country", newAddress.Country);
            cmd.Parameters.AddWithValue("@City", newAddress.City);
            cmd.Parameters.AddWithValue("@StreetName", newAddress.StreetName);
            cmd.Parameters.AddWithValue("@HouseNumber", newAddress.HouseNumber);
            cmd.Parameters.AddWithValue("@ApartmentNumber", newAddress.ApartmentNumber);
            cmd.Parameters.AddWithValue("@PostalCode", newAddress.PostalCode);

            con.Open();
            cmd.ExecuteNonQuery();
            con.Close();

            return Ok("Data inserted");     
        }
            
    
    }
}
