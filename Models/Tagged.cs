using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace BaseApi.Models {
    public class Tagged {
        [Required]
        public Guid BookId { get; set; }
        public Book Book { get; set; }

        [Required]
        public Guid TagId { get; set; }
        public Tag Tag { get; set; }

        public static void BuildConstraint (ModelBuilder builder) {
            builder.Entity<Tagged> ()
                .HasKey (t => new { t.TagId, t.BookId });
            builder.Entity<Tagged> ()
                .HasOne (t => t.Book)
                .WithMany (b => b.Taggeds)
                .HasForeignKey (t => t.BookId);
            builder.Entity<Tagged> ()
                .HasOne (t => t.Tag)
                .WithMany (t => t.Taggeds)
                .HasForeignKey (t => t.TagId);
        }
    }
}