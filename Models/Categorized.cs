using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace BaseApi.Models {
    public class Categorized {
        [Required]
        public Guid BookId { get; set; }
        public Book Book { get; set; }

        [Required]
        public Guid CategoryId { get; set; }
        public Category Category { get; set; }

        public static void BuildConstraint (ModelBuilder builder) {
            builder.Entity<Categorized> ()
                .HasKey (c => new { c.CategoryId, c.BookId });
            builder.Entity<Categorized> ()
                .HasOne (c => c.Book)
                .WithMany (b => b.Categorized)
                .HasForeignKey (c => c.BookId);
            builder.Entity<Categorized> ()
                .HasOne (c => c.Category)
                .WithMany (c => c.Categorized)
                .HasForeignKey (c => c.CategoryId);
        }
    }
}