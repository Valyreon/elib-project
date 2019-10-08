using System.ComponentModel.DataAnnotations;

namespace Domain
{
    public class UserCollection
    {
        public int Id { get; set; }
        [Required]
        public string Tag { get; set; }
    }
}