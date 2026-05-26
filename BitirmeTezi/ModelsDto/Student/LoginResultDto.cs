namespace BitirmeTezi.ModelsDto.Student
{
    public class LoginResultDto
    {
        public Guid UserId { get; set; } 
        public string Username { get; set; }
        public string NativeLanguage { get; set; }
        public string PasswordHash { get; set; }
        public bool IsActive { get; set; }
    }
}
