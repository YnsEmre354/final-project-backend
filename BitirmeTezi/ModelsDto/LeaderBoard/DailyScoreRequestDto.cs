using BitirmeTezi.Enums;

namespace BitirmeTezi.ModelsDto.LeaderBoard
{
    public class DailyScoreRequestDto
    {
        public int Points { get; set; }
        public SkillType SkillType { get; set; }
        public LevelType LevelType { get; set; }
    }
}
