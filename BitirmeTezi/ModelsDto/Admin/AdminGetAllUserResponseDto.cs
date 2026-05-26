namespace BitirmeTezi.ModelsDto.Admin
{
    public class AdminGetAllUserResponseDto
    {
        public string Name { get; set; }
        public string Surname { get; set; }

        public string Username { get; set; }

        public string Email { get; set; }
        public string NativeLanguage { get; set; }
        public string CreatedDate { get; set; }

        public bool IsActive { get; set; }
        public string Gender { get; set; }

        public string UserId { get; set; }

    }
}
