using BCrypt.Net;


namespace ecommerce_website_backend.Functions
{
    public class PasswordEncryption
    {

        public static string EncryptionedPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        public static bool VerifyPassword(string EnteredPassword, string HashedPassword )
        {
            
            return BCrypt.Net.BCrypt.Verify(EnteredPassword, HashedPassword);
        }
    }
}
