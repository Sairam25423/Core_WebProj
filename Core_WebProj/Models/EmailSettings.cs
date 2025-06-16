namespace Core_WebProj.Models
{
    public class EmailSettings
    {
        public required string SenderEmail { get; set; }
        public required string SenderName { get; set; }
        public required string SmtpServer { get; set; }
        public required int SmtpPort { get; set; }
        public required bool UseSSL { get; set; }
        public required string Password { get; set; }
    }
}
