using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace BitirmeTezi.Entities
{
    [Table("WritingQuestionTranslations")]
    public class WritingQuestionTranslation : BaseEntity
    {
        public int WritingQuestionId { get; set; }

        [ForeignKey("WritingQuestionId")]
        public virtual WritingQuestion? WritingQuestion { get; set; }

        [StringLength(10)]
        public required string LanguageCode { get; set; }

        public required string TranslatedText { get; set; } 
    }
}
