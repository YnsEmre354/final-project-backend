using BitirmeTezi.Data;
using BitirmeTezi.Interface;
using BitirmeTezi.ModelsDto.Student;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace BitirmeTezi.Repository
{
    public class StudentRepository : IStudentRepository
    {
        private readonly DataContext _context;
        public StudentRepository(DataContext context)
        {
            _context = context;
        }

        /// <summary>
        ///                  YARIN BURALAR DÜZENLENECEK BİRADER HA
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<List<StudentResultDto>> GetStudentsAsync()
        {
            try
            {
                var result = await _context.Database
                                   .SqlQueryRaw<StudentResultDto>("EXEC sp_GetActiveStudents")
                                   .ToListAsync();
                return result;
            }
            catch (SqlException e)
            {
                throw new Exception(e.Message);
            }
        }

        /*
                         var result = await _context.Database
                                   .SqlQuery<LoginResultDto>($"EXEC sp_loginStudent {email}")
                                   .ToListAsync();
         
         */
        public async Task<LoginResultDto> LoginAsync(string email)
        {
            try
            {
                var result = await _context.Database
                                   .SqlQuery<LoginResultDto>($"EXEC sp_loginStudent @Email={email}")
                                   .ToListAsync();

                return result.FirstOrDefault();
            }
            catch (SqlException e)
            {
                throw new Exception(e.Message);
            }
        }

        public async Task<int> RegisterAsync(string name, string surname, string username, string email, string password, string nativeLanguage, string gender)
        {
            try
            {

                var list = await _context.Database
                                    .SqlQuery<int>($"EXEC sp_AddStudent @Name={name}, @Surname={surname}, @Username={username}, @Email={email}, @PasswordHash={password}, @NativeLanguage={nativeLanguage}, @Gender={gender}")
                                    .ToListAsync();
                var result = list.FirstOrDefault();


                return result;
            }

            catch (SqlException e)
            {
                throw new Exception(e.Message);
            }
        }
        public async Task<LogStudentDto> GetLogStudentAsync(Guid userId)
        {
            try
            {
                var list = await _context.Database
                                .SqlQuery<LogStudentDto>($"EXEC sp_GetLogUser @UserId={userId}")
                                .ToListAsync();

                var result = list.FirstOrDefault();

                return result;
            }

            catch (SqlException e)
            {
                throw new Exception(e.Message);
            }
        }

        public async Task<int> ChangePasswordAsync(Guid userId, string oldPassword, string newPassword)
        {
            try
            {
                var list = await _context.Database
                                .SqlQuery<int>($"EXEC sp_changePassword @UserId={userId}, @OldPasswordHash={oldPassword}, @NewPasswordHash={newPassword}")
                                .ToListAsync();

                var result = list.FirstOrDefault();

                return result;
            }

            catch (SqlException e)
            {
                throw new Exception(e.Message);
            }
        }

        public async Task<int> ChangeUsernameAsync(Guid userId, string newUsername)
        {
            try
            {
                var list = await _context.Database
                                .SqlQuery<int>($"EXEC sp_changeUsername @UserId={userId}, @Username={newUsername}")
                                .ToListAsync();
                var result = list.FirstOrDefault();

                return result;
            }
            catch (SqlException e)
            {
                throw new Exception(e.Message);
            }
        }

        public async Task<int> DeleteUserAsync(Guid userId)
        {
            try
            {
                var list = await _context.Database
                                .SqlQuery<int>($"EXEC sp_DeleteUser @UserId={userId}")
                                .ToListAsync();
                var result = list.FirstOrDefault();

                return result;
            }
            catch (SqlException e)
            {
                throw new Exception(e.Message);
            }
        }
    }
}
