using BitirmeTezi.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace BitirmeTezi.Entities
{
    [Table("UserSkillEntrollment")]
    public class UserSkillEnrollment: BaseEntity
    {
        public int StudentId { get; set; }
        public SkillType SkillType { get; set; }
        public LevelType LevelType { get; set; }
        public bool IsLocked { get; set; } = true;
        public bool IsAttempted { get; set; } = false;
    }
}
