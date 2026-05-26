using BitirmeTezi.Enums;

namespace BitirmeTezi.ModelsDto.Student
{
    public class StudentAnswerDto
    {
        public int LevelType { get; set; }
        public string? StudentAnswer { get; set; }
        public string? CorrectAnswer { get; set; }
        public int SkillType { get; set; }

    }
}
