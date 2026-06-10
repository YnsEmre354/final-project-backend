namespace BitirmeTezi.ModelsDto.Student
{
    public class CompleteFirebaseRegisterRequestDto
    {
        public string IdToken { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Surname { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string NativeLanguage { get; set; } = string.Empty;
        public string Gender { get; set; } = string.Empty;
    }
}
