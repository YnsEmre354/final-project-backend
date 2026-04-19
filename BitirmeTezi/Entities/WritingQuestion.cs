using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BitirmeTezi.Entities
{
    [Table("WritingQuestions")]
    public class WritingQuestion : BaseQuestion
    {
        [StringLength(200)]
        public required string CorrectAnswer { get; set; }

        public ICollection<WritingQuestionTranslation> Translations { get; set; }

    }
}
