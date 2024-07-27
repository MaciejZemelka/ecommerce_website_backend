namespace ecommerce_website_backend.Functions
{
    public class PasswordEncryption
    {

        public static string EncryptionedPassword(string pass)
        {
            Random rnd = new Random();
            char[] password = pass.ToCharArray();
            char[] encrypted = new char[pass.Length * 3];
            char letter;
            for (int i = 1, h = 0; i <= pass.Length * 3; i++)
            {
                if (i % 3 == 0)
                {
                    int d = password[h] - '0';
                    letter = (char)d;
                    encrypted[i - 1] = letter;
                    h++;
                }
                else
                {
                    int start = 40;
                    int end = 122;
                    int Rolled = rnd.Next(start, end);
                    encrypted[i - 1] = (char)Rolled;

                }

            }
            string result = new string(encrypted);
            return result;
        }

        public static string DecryptionedPassword(string pass)
        {
            char[] password = pass.ToCharArray();
            char[] decrypted = new char[pass.Length / 3];
            char letter;
            for (int i = 1, h = 0; i <= password.Length; i++)
            {
                if (i % 3 == 0)
                {
                    int d = password[i - 1] + '0';
                    letter = (char)d;
                    decrypted[h] = letter;
                    h++;
                }

            }
            string result = new string(decrypted);
            return result;
        }
    }
}
