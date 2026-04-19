using System.ComponentModel.DataAnnotations.Schema;

namespace BitirmeTezi.Entities
{
    [Table("StudentEnrollments")]

    public class StudentEnrollment: BaseEntity
    {
        [ForeignKey("StudentId")]
        public int StudentId { get; set; }
        public virtual Student? Student { get; set; }

        [ForeignKey("CourseId")]
        public int CourseId { get; set; }
        public virtual Course? Course { get; set; }
    }
}
