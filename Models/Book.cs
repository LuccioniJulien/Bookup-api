using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
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

        public static async Task<bool> SaveNewBookFromGoogle (string isbn) {
            try {
                using (var client = new HttpClient ()) {
                    string url = $"https://www.googleapis.com/books/v1/volumes?q=isbn={isbn}";
                    string json = await client.GetStringAsync (url);
                    var resource = JObject.Parse (json);
                    var item = resource["items"][0];
                    var livre = GetBookFromJSON (item);
                    if (livre.Isbn != isbn) {
                        return false;
                    }
                    using (var context = new DBcontext ()) {
                        var newAuthors = Author.GetAuthorsFromJSON (item).Select (x => {
                            var author = context.Authors.FirstOrDefault (a => a.Name == x.Name);
                            if (author != null) return author;
                            context.Add (x);
                            context.SaveChanges ();
                            return x;
                        });
                        var newCategories = Category.GetCategoriesFromJSON (item).Select (x => {
                            var category = context.Categories.FirstOrDefault (a => a.Name == x.Name);
                            if (category != null) return category;
                            context.Add (x);
                            context.SaveChanges ();
                            return x;
                        });
                        context.Add (livre);
                        context.SaveChanges ();

                        newAuthors.Select (a => a.Id).ToList ().ForEach (x => {
                            context.Add (new Written { BookId = livre.Id, AuthorId = x });
                        });
                        newCategories.Select (c => c.Id).ToList ().ForEach (x => {
                            context.Add (new Categorized { BookId = livre.Id, CategoryId = x });
                        });
                        context.SaveChanges ();
                    }
                }
                return true;
            } catch (Exception e) {
                Console.WriteLine (e.Message);
                return false;
            }

        }
        public static Book GetBookFromJSON (JToken json) {
            var isbn = json["volumeInfo"]["industryIdentifiers"];
            if (isbn == null) return null;
            isbn = (string) isbn.ToList () [0]["type"] == "ISBN_13" ? isbn.ToList () [0]["identifier"] : isbn.ToList () [1]["identifier"];

            DateTime.TryParseExact ((string) json["volumeInfo"]["publishedDate"], "yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime dt);
            return new Book {
                Isbn = (string) isbn,
                    Title = (string) json["volumeInfo"]["title"],
                    Description = (string) json["volumeInfo"]["description"],
                    Thumbnail = (string) json?["volumeInfo"] ? ["imageLinks"] ? ["thumbnail"],
                    PublishedDate = dt
            };
        }
    }
}