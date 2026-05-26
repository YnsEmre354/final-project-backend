using BitirmeTezi.Enums;

namespace BitirmeTezi.ModelsDto.Student
{
    public class UnlockLevelRequestDto
    {
        public SkillType SkillType { get; set; }
        public LevelType CurrentLevel { get; set; }
        public LevelType NextLevel { get; set; }
    }
}
