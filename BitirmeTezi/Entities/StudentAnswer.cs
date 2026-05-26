using BitirmeTezi.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace BitirmeTezi.Entities
{
    [Table("StudentEnrollments")]

    public class StudentAnswer : BaseEntity
    {
        [ForeignKey("StudentId")]
        public int StudentId { get; set; }
        public virtual Student? Student { get; set; }

        public SkillType Skill { get; set; }
        public LevelType Level { get; set; }


        public string? StudentAnswerText { get; set; }

        public bool? IsCorrect { get; set; }

        public double? AIScore { get; set; }

    }
}
