using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;

namespace BaseApi.Models {
    public class Book {
        public Guid Id { get; set; }

        [Required]
        public string Isbn { get; set; }

        [Required]
        public string Title { get; set; }
        public string Description { get; set; }
        public string Thumbnail { get; set; }
        public DateTime PublishedDate { get; set; }
        public ICollection<Tagged> Taggeds { get; set; }
        public ICollection<Written> Writtens { get; set; }
        public ICollection<Categorized> Categorized { get; set; }
        public static void BuildConstraint (ModelBuilder builder) {
            builder.Entity<Book> ()
                .HasIndex (u => u.Isbn)
                .IsUnique ();
        }
        public static Book GetBookFromJSON (JToken json) {
            var isbn = json["volumeInfo"]["industryIdentifiers"];
            if (isbn == null) return null;
            isbn = isbn.ToList () [0]["identifier"];

            DateTime.TryParseExact ((string) json["volumeInfo"]["publishedDate"], "yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime dt);
            return new Book {
                Isbn = (string) isbn,
                    Title = (string) json["volumeInfo"]["title"],
                    Description = (string) json["volumeInfo"]["description"],
                    Thumbnail = (string) json["volumeInfo"]["imageLinks"]["thumbnail"],
                    PublishedDate = dt
            };
        }
    }
}