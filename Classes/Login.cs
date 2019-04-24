using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BaseApi.Classes {
    public class Login {

        [EmailAddress, Required]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }
        public void Deconstruct (out string password, out string email) {
            password = Password;
            email = Email;
        }
    }
}