namespace BitirmeTezi.Auth
{
    public class EmailSettings
    {
        public string SmtpHost { get; set; } = string.Empty;
        public int SmtpPort { get; set; } = 587;
        public string SenderEmail { get; set; } = string.Empty;
        public string SenderName { get; set; } = string.Empty;
        public string SenderPassword { get; set; } = string.Empty;
        public bool UseSsl { get; set; } = true;

        /// <summary>
        /// Flutter Web URL — mail linklerinin base'i.
        /// Örnek: http://localhost:8080
        /// </summary>
        public string FrontendBaseUrl { get; set; } = string.Empty;
    }
}
