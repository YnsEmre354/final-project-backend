namespace BitirmeTezi.ModelsDto.Student
{
    public class ChangePasswordRequestDto
    {
        public required string OldPassword { get; set; }
        public required string NewPassword { get; set; }
    }

}
