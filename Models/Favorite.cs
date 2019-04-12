using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace BaseApi.Models {
    public class Favorite {
        [Required]
        public Guid UserId { get; set; }
        public User User { get; set; }

        [Required]
        public Guid TagId { get; set; }
        public Tag Tag { get; set; }
        public static void BuildConstraint (ModelBuilder builder) {
            builder.Entity<Favorite> ()
                .HasKey (f => new { f.TagId, f.UserId });
            builder.Entity<Favorite> ()
                .HasOne (f => f.User)
                .WithMany (u => u.Favorites)
                .HasForeignKey (f => f.UserId);
            builder.Entity<Favorite> ()
                .HasOne (f => f.Tag)
                .WithMany (t => t.Favorites)
                .HasForeignKey (t => t.TagId);
        }
    }
}