using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BaseApi.Classes {
    [NotMapped]
    public class PasswordHelper {
        [Required]
        public string Password { get; set; }

        [Required, Compare ("Password")]
        public string PasswordConfirmation { get; set; }

        public void Deconstruct (out string password, out string passwordConfirmation) {
            password = Password;
            passwordConfirmation = PasswordConfirmation;
        }
    }
}