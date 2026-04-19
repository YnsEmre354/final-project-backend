using BitirmeTezi.Entities;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Claims;

namespace BitirmeTezi.Entities
{
    [Table("Courses")]
    public class Course : BaseEntity
    {
        [Required]
        [StringLength(100)]
        public required string Title { get; set; } 

        [StringLength(500)]
        public required string Description { get; set; }

        public int Level { get; set; }   

    }
}