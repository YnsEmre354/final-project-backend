namespace BitirmeTezi.ModelsDto.Student
{
    public class LogStudentDto
    {
        public required string Name { get; set; }
        public required string Surname { get; set; }
        public required string Username { get; set; }
        public required string Email { get; set; }
        public required string NativeLanguage { get; set; }
        public string? ProfilePicture { get; set; }
        public string? Gender { get; set; }
        public required bool IsActive { get; set; }
        public required Guid UserId { get; set; }

    }
}
