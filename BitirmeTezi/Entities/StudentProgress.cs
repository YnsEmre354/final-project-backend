using BitirmeTezi.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace BitirmeTezi.Entities
{
    [Table("StudentProgress")]
    public class StudentProgress : BaseEntity
    {
        [ForeignKey("StudentId")]
        public int StudentId { get; set; }
        public Student? Student { get; set; }

        public SkillType Skill { get; set; }
        public LevelType Level { get; set; }

        public int CorrectAnswers { get; set; }

        public int TotalQuestions { get; set; }

        public double AverageScore { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedDate { get; set; }
        public StatusType Status { get; set; }
        public bool IsPassed { get; set; }
        public double ProgressPercentage { get; set; }

    }
}
