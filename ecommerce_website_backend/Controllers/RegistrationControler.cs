using Chat.Server.models;
using ecommerce_website_backend.Functions;
using ecommerce_website_backend.models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Security.AccessControl;

namespace Chat.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RegistrationController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public RegistrationController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpPost]
        [Route("registration")]
        public string Registration(Registration registration)
        {
            SqlConnection con = new SqlConnection(_configuration.GetConnectionString("ecommerce_DBcon"));
            string password = PasswordEncryption.EncryptionedPassword(registration.Password);
            SqlCommand cmd = new SqlCommand("INSERT INTO users(Username,Email,Password,Permission, CreatedDate) VALUES('" + registration.Username + "','" + registration.Email + "','" + password + "','" + registration.Permission + "','"+registration.CreatedDate+"')", con);
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
        public string Login(Login login)
        {
            SqlConnection con = new SqlConnection(_configuration.GetConnectionString("ecommerce_DBcon"));
            SqlCommand cmd = new SqlCommand("Select count(*) from Users where Email='" + login.Email+"'", con);
            con.Open();
            int i = (int)cmd.ExecuteScalar();
            if(i == 0)
            {
                return "No user";
            }
            else
            {
                SqlCommand cmd2 = new SqlCommand("Select Password from Users where Email ='" + login.Email+"'", con);
                using (SqlDataReader reader = cmd2.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        string password2 = String.Format("{0}", reader["password"]);
                        string checkPass = PasswordEncryption.DecryptionedPassword(password2);
                        if(checkPass == login.Password)
                        {
                            return "Valid User";
                        }
                        else
                        {
                            return "Wrong password";
                        }

                    }
                }
            }
            con.Close();
            return "Server error";
        }
    }
}
