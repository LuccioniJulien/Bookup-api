using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BaseApi.Helper;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace BaseApi.Models {
    public class User {
        public Guid Id { get; set; }

        [Required, MinLength (5), MaxLength (20)]
        public string Name { get; set; }
        public string Avatar_url { get; set; }

        [EmailAddress, Required]
        public string Email { get; set; }
        public string PasswordHash { get; set; }

        [Required, NotMapped]
        public string Password { get; set; }

        [Required, Compare ("Password"), NotMapped]
        public string PasswordConfirmation { get; set; }

        public ICollection<Favorite> Favorites { get; set; }

        public static void BuildConstraint (ModelBuilder builder) {
            builder.Entity<User> ()
                .Property (u => u.PasswordHash)
                .IsRequired ();
            builder.Entity<User> ()
                .HasIndex (u => u.Email)
                .IsUnique ();
        }
        public void SetPasswordhHash () => PasswordHash = BCrypt.Net.BCrypt.HashPassword (input: Password);
        public bool Compare (string password) => BCrypt.Net.BCrypt.Verify (password, PasswordHash);

        public void SetAvatar (string name) => Avatar_url = "https://s3.eu-west-3.amazonaws.com/bookupstorapeapi/" + name;
        public object ToMessage () => new { Id, Name, Email, Avatar_url };
        public void Deconstruct (out string name, out string mail, out string password, out string passwordConfirmation) {
            name = Name;
            password = Password;
            mail = Email;
            passwordConfirmation = PasswordConfirmation;
        }
    }
}