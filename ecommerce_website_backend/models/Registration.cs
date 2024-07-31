namespace Chat.Server.models
{
    public class Registration
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string Permission { get; set; }
        public string CreatedDate { get; set; }
    }


}
