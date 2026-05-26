using BitirmeTezi.Data;
using BitirmeTezi.Interface;
using BitirmeTezi.ModelsDto.Admin;
using BitirmeTezi.ModelsDto.Student;
using Microsoft.EntityFrameworkCore;

namespace BitirmeTezi.Repository
{
    public class AdminRepository: IAdminRepository
    {
        private readonly DataContext _context;
        public AdminRepository(DataContext context)
        {
            _context = context;
        }

        public async Task<bool> LoginAdmin(LoginRequestDto dto)
        {
            try
            {
                var admin = await _context.Admins
                    .FirstOrDefaultAsync(a => a.Email == dto.Email && a.Password == dto.Password);

                if(admin == null)
                {
                    return false;
                }

                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine("Hata :" + e.Message);
                return false;
            }
        }

        public async Task<List<AdminGetAllUserResponseDto>> GetAllUser()
        {
            try
            {
                return await _context.Students
                .Select(u => new AdminGetAllUserResponseDto
                {
                    UserId = u.UserId.ToString(), 
                    Surname = u.Surname,
                    Username = u.Username,
                    Email = u.Email,
                    NativeLanguage = u.NativeLanguage,
                    CreatedDate = u.CreatedDate.ToString("dd.MM.yyyy"), 
                    IsActive = u.IsActive,
                    Gender = u.Gender
                }).ToListAsync();
            }

            catch (Exception e)
            {
                Console.WriteLine("Hata : " + e.Message);
                return new List<AdminGetAllUserResponseDto>();
            }
        }

        public async Task<List<AdminStudentProgressResponse>> GetStudentProgressDetails(string userId)
        {
            try
            {
                Guid GuidUserId = Guid.Parse(userId);
                var studentId = await _context.Students
                                    .Where(s => s.UserId == GuidUserId)
                                    .Select(s => s.Id)
                                    .FirstOrDefaultAsync();
                if (studentId == 0) return new List<AdminStudentProgressResponse>();

                var progressList = await _context.StudentProgress
                    .Where(p => p.StudentId == studentId)
                    .Select(p => new AdminStudentProgressResponse
                    {
                        SkillType = p.Skill,
                        LevelType = p.Level,
                        CorrectAnswers = p.CorrectAnswers,
                        TotalQuestions = p.TotalQuestions,
                        AverageScore = p.AverageScore,
                        IsPassed = p.IsPassed,
                        ProgressPercentage = p.ProgressPercentage,
                        LastUpdated = p.UpdatedDate.ToString("dd.MM.yyyy")
                    })
                    .ToListAsync();

                return progressList;
            }
            catch (Exception e)
            {
                Console.WriteLine("İlerleme çekilirken hata: " + e.Message);
                return new List<AdminStudentProgressResponse>(); 
            }
        }

        public async Task<bool> SoftDeleteUser(string userId)
        {
            try
            {
                if (!Guid.TryParse(userId, out Guid GuidUserId))
                {
                    return false; 
                }

                var student = await _context.Students
                                    .FirstOrDefaultAsync(s => s.UserId == GuidUserId);

                if(student == null)  return false;

                student.IsActive = false;

                var progress = await _context.StudentProgress
                     .Where(p => p.StudentId == student.Id).ToListAsync();

                foreach (var p in progress)
                {
                    p.IsActive = false;
                }

                await _context.SaveChangesAsync();

                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine("Kullanıcı silerken hata: " + e.Message);
                return false;
            }
        }

        public async Task<bool> ActiveUser(string userId)
        {
            try
            {
                if(!Guid.TryParse(userId, out Guid GuidUserId))
                {
                    return false;
                }

                var student = await _context.Students
                                    .FirstOrDefaultAsync(s => s.UserId == GuidUserId);

                if (student == null) return false;

                student.IsActive = true;


                var progress = await _context.StudentProgress
                                     .Where(p => p.StudentId == student.Id).ToListAsync();

                foreach(var p in progress)
                {
                    p.IsActive = true;
                }

                await _context.SaveChangesAsync();

                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine("Kullanıcı aktif ederken hata: " + e.Message);
                return false;
            }
        }
    }
}
