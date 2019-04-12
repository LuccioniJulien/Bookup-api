using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace BaseApi.Models {
    public class Written {
        [Required]
        public Guid BookId { get; set; }
        public Book Book { get; set; }

        [Required]
        public Guid AuthorId { get; set; }
        public Author Author { get; set; }
        public static void BuildConstraint (ModelBuilder builder) {
            builder.Entity<Written> ()
                .HasKey (w => new { w.BookId, w.AuthorId });
            builder.Entity<Written> ()
                .HasOne (w => w.Book)
                .WithMany (b => b.Writtens)
                .HasForeignKey (w => w.BookId);
            builder.Entity<Written> ()
                .HasOne (w => w.Author)
                .WithMany (a => a.Writtens)
                .HasForeignKey (w => w.AuthorId);
        }
    }
}