using BitirmeTezi.Data;
using BitirmeTezi.Entities;
using BitirmeTezi.Enums;

namespace BitirmeTezi.Service
{
    public class UserService
    {
        public async Task InitializeUserSkillEnrollemntAsync(int studentId, DataContext _context)
        {
            var enrollments = new List<UserSkillEnrollment>();
            var skillTypes = Enum.GetValues<SkillType>();
            var levelTypes = Enum.GetValues<LevelType>();

            foreach(var skill in skillTypes)
            {
                foreach(var level in levelTypes)
                {
                    enrollments.Add(new UserSkillEnrollment
                    {
                        StudentId = studentId,
                        SkillType = skill,
                        LevelType = level,
                        IsLocked = level != LevelType.A1,
                        IsAttempted = false
                    });
                }
            }

            await _context.UserSkillEnrollments.AddRangeAsync(enrollments);
            await _context.SaveChangesAsync();
        }
    }
}
