namespace BitirmeTezi.ModelsDto.Student
{
    public class GeneralStudentSubmitWSDto
    {
        public int LevelType { get; set; }
        public int SkillType { get; set; }
        public int StatusType { get; set; }
        public List<int> AiScores { get; set; } = new List<int>();
        public int TotalCount { get; set; }
    }
}
