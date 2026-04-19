using System.ComponentModel.DataAnnotations;

namespace BitirmeTezi.Entities
{
    //ORTAK CLASS 
    public class BaseEntity
    {
        [Key]
        public int Id { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public DateTime? UpdatedDate { get; set; }
        public bool IsActive { get; set; } = true;

    }
}
