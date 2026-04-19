using System.ComponentModel.DataAnnotations.Schema;

namespace BitirmeTezi.Entities
{
    [Table("StudentProgress")]
    public class StudentProgress : BaseEntity
    {
        [ForeignKey("StudentId")]
        public int StudentId { get; set; }
        public Student? Student { get; set; }

        [ForeignKey("CourseId")]
        public int CourseId { get; set; }
        public Course? Course { get; set; }

        [ForeignKey("LessonId")]
        public int LessonId { get; set; }
        public Lesson? Lesson { get; set; }

        [ForeignKey("WritingQuestionId")]
        public int WritingQuestionId { get; set; }

        [ForeignKey("SpeakingQuestionId")]
        public int SpeakingQuestionId { get; set; }

        [ForeignKey("ListeningQuestionId")]
        public int ListeningQuestionId { get; set; }

        public string? WritingStudentAnswer { get; set; }
        public string? SpeakingStudentAnswer { get; set; }

        public string? ListeningStudentAnswer { get; set; }

        public double AIScore { get; set; }
        public string? AIFeedBack { get; set; }

    }
}
