using System.ComponentModel.DataAnnotations.Schema;

namespace BitirmeTezi.Entities
{
    [Table("BaseQuestions")]
    public abstract class BaseQuestion: BaseEntity
    {
        [ForeignKey("LessonId")]
        public int LessonId { get; set; }
        public virtual Lesson? Lesson { get; set; }

        public string? GrammarTag { get; set; }

    }
}
