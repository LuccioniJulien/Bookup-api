using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BaseApi.Classes {
    [NotMapped]
    public class InfoUser {
        [Required, MinLength (5), MaxLength (20)]
        public string Name { get; set; }

        [EmailAddress, Required]
        public string Email { get; set; }

        public void Deconstruct (out string name, out string email) {
            name = Name;
            email = Email;
        }
    }
}