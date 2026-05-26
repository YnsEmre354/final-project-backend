using BitirmeTezi.Enums;

namespace BitirmeTezi.ModelsDto.Admin
{
    public class AdminStudentProgressResponse
    {
        public int SkillId { get; set; }
        public SkillType SkillType { get; set; } 
        public LevelType LevelType { get; set; }
        public int CorrectAnswers { get; set; }
        public int TotalQuestions { get; set; }
        public double AverageScore { get; set; }
        public bool IsPassed { get; set; }
        public double ProgressPercentage { get; set; }
        public string LastUpdated { get; set; }
    }
}
