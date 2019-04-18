using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using BaseApi.Helper;
using Microsoft.AspNetCore.Hosting.Internal;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;

namespace BaseApi.Models {
    public class DBcontext : DbContext {
        public DbSet<User> Users { get; set; }
        public DbSet<Author> Authors { get; set; }
        public DbSet<Favorite> Favorites { get; set; }
        public DbSet<Tag> Tags { get; set; }
        public DbSet<Tagged> Taggeds { get; set; }
        public DbSet<Written> Writtens { get; set; }
        public DbSet<Book> Books { get; set; }
        public DbSet<Categorized> Categorizeds { get; set; }
        public DbSet<Category> Categories { get; set; }
        protected override void OnModelCreating (ModelBuilder builder) {
            User.BuildConstraint (builder);
            Favorite.BuildConstraint (builder);
            Tagged.BuildConstraint (builder);
            Written.BuildConstraint (builder);
            Book.BuildConstraint (builder);
            Categorized.BuildConstraint (builder);
        }

        protected override void OnConfiguring (DbContextOptionsBuilder optionsBuilder) {
            var (dbHost, dbName, dbUser, dbPassword) = new DbConfig ();
            optionsBuilder.UseNpgsql ($"Host={dbHost};Database={dbName};Username={dbUser};Password={dbPassword}");
        }

        public async void Seed () {
            using (var client = new HttpClient ()) {
                var urls = new List<string> {
                "https://www.googleapis.com/books/v1/volumes?q=Fantasy&langRestrict=en&maxResults=40",
                "https://www.googleapis.com/books/v1/volumes?q=Science+Fiction&langRestrict=en&maxResults=40",
                "https://www.googleapis.com/books/v1/volumes?q=History&langRestrict=en&maxResults=40",
                "https://www.googleapis.com/books/v1/volumes?q=Drama&langRestrict=en&maxResults=40",
                "https://www.googleapis.com/books/v1/volumes?q=Science&langRestrict=en&maxResults=40",
                "https://www.googleapis.com/books/v1/volumes?q=Fiction&langRestrict=en&maxResults=40",
                "https://www.googleapis.com/books/v1/volumes?q=Heroic&langRestrict=en&maxResults=40",
                "https://www.googleapis.com/books/v1/volumes?q=Vagner&langRestrict=en&maxResults=40",
                "https://www.googleapis.com/books/v1/volumes?q=Horor&langRestrict=en&maxResults=40",
                "https://www.googleapis.com/books/v1/volumes?q=games+of+thrones&langRestrict=en&maxResults=40",
                "https://www.googleapis.com/books/v1/volumes?q=star+wars&langRestrict=en&maxResults=40",
                "https://www.googleapis.com/books/v1/volumes?q=star+trek&langRestrict=en&maxResults=40",
                "https://www.googleapis.com/books/v1/volumes?q=dystopia&langRestrict=en&maxResults=40",
                "https://www.googleapis.com/books/v1/volumes?q=uchronic&langRestrict=en&maxResults=40",
                "https://www.googleapis.com/books/v1/volumes?q=SMITH+Dan&langRestrict=en&maxResults=40",
                "https://www.googleapis.com/books/v1/volumes?q=Ormston+Dean&langRestrict=en&maxResults=40",
                "https://www.googleapis.com/books/v1/volumes?q=Infinity+Wars&langRestrict=en&maxResults=40",
                "https://www.googleapis.com/books/v1/volumes?q=comics&langRestrict=en&maxResults=40",
                "https://www.googleapis.com/books/v1/volumes?q=dc&langRestrict=en&maxResults=40"
                };

                foreach (var url in urls) {
                    string json = await client.GetStringAsync (url);
                    var resource = JObject.Parse (json);
                    Insert (resource);
                }
            }
        }

        public void Insert (JObject resource) {
            resource["items"].Select (t => t).ToList ().ForEach (i => {
                try {

                    using (var context = new DBcontext ()) {
                        var newAuthors = Author.GetAuthorsFromJSON (i).Select (x => {
                            var author = context.Authors.FirstOrDefault (a => a.Name == x.Name);
                            if (author != null) return author;
                            context.Add (x);
                            context.SaveChanges ();
                            return x;
                        });

                        var newCategories = Category.GetCategoriesFromJSON (i).Select (x => {
                            var category = context.Categories.FirstOrDefault (a => a.Name == x.Name);
                            if (category != null) return category;
                            context.Add (x);
                            context.SaveChanges ();
                            return x;
                        });

                        var livre = Book.GetBookFromJSON (i);
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

                } catch (Exception e) {
                    // Console.WriteLine (e.Message);
                }

            });
        }

    }
}