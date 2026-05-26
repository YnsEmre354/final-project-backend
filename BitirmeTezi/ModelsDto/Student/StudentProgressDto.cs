using BitirmeTezi.Enums;

namespace BitirmeTezi.ModelsDto.Student
{
    public class StudentProgressDto
    {
        public int StudentId { get; set; }
        public int CorrectAnswers { get; set; }
        public int TotalQuestions { get; set; }
        public int LevelType { get; set; }
        public int SkillType { get; set; }
        public int StatusType { get; set; }
    }
}
