using BitirmeTezi.Entities;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Claims;

namespace BitirmeTezi.Entities
{
    [Table("Lessons")]
    public class Lesson : BaseEntity
    {
        [ForeignKey("CourseId")]
        public int CourseId { get; set; }
        public virtual Course? Course { get; set; }

        public required string Title { get; set; } 

        public int Order { get; set; }

        public required string Content { get; set; }

    }
}