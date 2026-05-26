using System.ComponentModel;
using System.Text.Json.Serialization;

namespace BitirmeTezi.ModelsDto.Student
{
    public class StudentResultDto
    {
        public required string Name { get; set; }
        public required string Email { get; set; }
        public Guid UserId { get; set; }
        public DateTime CreatedDate { get; set; }

    }
}
