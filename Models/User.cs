using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BaseApi.Helper;
using Microsoft.IdentityModel.Tokens;

namespace BaseApi.Models {
    public class User {
        public Guid Id { get; set; }

        [Required, MinLength (5)]
        public string Name { get; set; }

        [EmailAddress, Required]
        public string Email { get; set; }
        public string PasswordHash { get; set; }

        [Required, NotMapped]
        public string Password { get; set; }

        [Required, Compare ("Password"), NotMapped]
        public string PasswordConfirmation { get; set; }

        public void SetPasswordhHash () => PasswordHash = BCrypt.Net.BCrypt.HashPassword (input: Password);

        public bool Compare (string password) => BCrypt.Net.BCrypt.Verify (password, PasswordHash);

        public object ToMessage () => new { Id, Name, Email };

        public void Deconstruct (out string name,out string mail, out string password, out string passwordConfirmation) {
            name = Name;
            password = Password;
            mail = Email;
            passwordConfirmation = PasswordConfirmation;
        }
    }
}