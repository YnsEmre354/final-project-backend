namespace BitirmeTezi.ModelsDto.Student
{
    public class RegisterRequestDto
    {
        public required string Name { get; set; }
        public required string Surname { get; set; }

        public required string Username { get; set; }

        public required string Email { get; set; }

        public required string Password { get; set; }

        public required string NativeLanguage { get; set; }
        public string Gender { get; set; }
    }
}
