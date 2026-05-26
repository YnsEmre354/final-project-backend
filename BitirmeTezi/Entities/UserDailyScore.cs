using BitirmeTezi.Enums;
using System.ComponentModel.DataAnnotations;

namespace BitirmeTezi.Entities
{
    public class UserDailyScore
    {
        [Key]
        public int Id { get; set; }
        public int UserId { get; set; }
        public int Points { get; set; }
        public SkillType SkillType { get; set; }
        public LevelType LevelType { get; set; }
        public DateTime EarnDate { get; set; }
    }
}
