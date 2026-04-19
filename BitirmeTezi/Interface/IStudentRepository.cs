using BitirmeTezi.ModelsDto.Student;

namespace BitirmeTezi.Interface
{
    public interface IStudentRepository
    {
        Task<LoginResultDto> LoginAsync(string email);
        Task<List<StudentResultDto>> GetStudentsAsync();
        Task<int> RegisterAsync(string name, string surname, string username, string email, string password, string nativeLanguage, string gender);
        Task<LogStudentDto> GetLogStudentAsync(Guid userId);
        Task<int> ChangePasswordAsync(Guid userId, string oldPassword ,string newPassword);
        Task<int> ChangeUsernameAsync(Guid userId, string newUsername);
        Task<int> DeleteUserAsync(Guid userId);
    }
}
