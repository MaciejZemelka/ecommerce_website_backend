using Chat.Server.models;
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
            SqlCommand cmd = new SqlCommand("INSERT INTO users(Username,Email,Password,Permission) VALUES('" + registration.Username + "','" + registration.Email + "','" + registration.Password + "','" + registration.Permission + "')", con);
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
            using (SqlConnection con = new SqlConnection(_configuration.GetConnectionString("ecommerce_DBcon")))
            {
                string query = "SELECT * FROM users WHERE Email = @Email AND Password = @Password";
                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@Email", login.Email);
                cmd.Parameters.AddWithValue("@Password", login.Password);

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                if (dt.Rows.Count > 0)
                {
                    return "Valid User";
                }
                else
                {
                    return "Invalid User";
                }
            }
        }
    }
}
