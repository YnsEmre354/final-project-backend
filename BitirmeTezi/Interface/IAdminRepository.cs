using BitirmeTezi.ModelsDto.Admin;
using BitirmeTezi.ModelsDto.Student;

namespace BitirmeTezi.Interface
{
    public interface IAdminRepository
    {
        Task<bool> LoginAdmin(LoginRequestDto dto);
        Task<List<AdminGetAllUserResponseDto>> GetAllUser();
        Task<List<AdminStudentProgressResponse>> GetStudentProgressDetails(string userId);
        Task<bool> SoftDeleteUser(string userId);
        Task<bool> ActiveUser(string userId);
    }
}
