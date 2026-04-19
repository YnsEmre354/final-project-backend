using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BitirmeTezi.Entities
{
    [Table("Students")]
    public class Student : BaseEntity
    {
        [StringLength(50)] 
        public required string Name { get; set; }

        [StringLength(50)]
        public required string Surname { get; set; }

        [StringLength(20)]
        public required string Username { get; set; }

        [EmailAddress] 
        [StringLength(100)]
        public required string Email { get; set; }

        public required string PasswordHash { get; set; }
        
        public string? NativeLanguage { get; set; }
        public string? ProfilePicture { get; set; }
        public string? Gender { get; set; }
        public Guid UserId { get; set; } = Guid.NewGuid();

    }
}
