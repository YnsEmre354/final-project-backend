using BitirmeTezi.Entities;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace BitirmeTezi.Entities
{
    [Table("Admins")]
    public class Admin : BaseEntity
    {
        [StringLength(50)]
        public required string Name { get; set; }

        [StringLength(50)]
        public required string Surname { get; set; }

        [EmailAddress]
        [StringLength(100)]
        public required string Email { get; set; }

        public required string Password { get; set; }

    }
}