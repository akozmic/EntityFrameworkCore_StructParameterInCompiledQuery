using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EFBug
{
    [Table("User")]
    class User
    {
        [Key,DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int UserId { get; set; }

        public string FirstName { get; set; }
        public string LastName { get; set; }
    }
}