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
        protected override void OnModelCreating (ModelBuilder builder) {
            User.BuildConstraint (builder);
            Favorite.BuildConstraint (builder);
            Tagged.BuildConstraint (builder);
            Written.BuildConstraint (builder);
        }

        protected override void OnConfiguring (DbContextOptionsBuilder optionsBuilder) {
            var (dbHost, dbName, dbUser, dbPassword) = new DbConfig ();
            optionsBuilder.UseNpgsql ($"Host={dbHost};Database={dbName};Username={dbUser};Password={dbPassword}");
        }

        public async void Seed () {
            using (var client = new HttpClient ()) {
                string json = await client.GetStringAsync ("https://www.googleapis.com/books/v1/volumes?q=%22%22&langRestrict=en");
                var resource = JObject.Parse (json);
                // string cpu = (string)o["CPU"];
                // // Intel
                // string firstDrive = (string)o["Drives"][0];
                // // DVD read/writer
                // IList<string> allDrives = o["Drives"].Select(t => (string)t).ToList();

                resource["items"].Select (t => t).ToList ().ForEach (i => {
                    try {
                        var isbn = i["volumeInfo"]["industryIdentifiers"];
                        if (isbn == null) return;
                        isbn = isbn.ToList () [0]["identifier"];

                        // if (string.IsNullOrEmpty (isbn)) return;
                        DateTime.TryParseExact ((string) i["volumeInfo"]["publishedDate"], "yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime dt);
                        var livre = new Book {
                            Isbn = (string) isbn,
                            Title = (string) i["volumeInfo"]["title"],
                            Description = (string) i["volumeInfo"]["description"],
                            Thumbnail = (string) i["volumeInfo"]["imageLinks"]["thumbnail"],
                            PublishedDate = dt
                        };
                        using (var context = new DBcontext ()) {
                            var newAuthors = new List<Author> ();
                            i["volumeInfo"]["authors"].ToList ().ForEach (x => {
                                var tempAuthor = new Author { Name = (string) x };
                                context.Add (tempAuthor);
                                context.SaveChanges ();
                                newAuthors.Add (tempAuthor);
                            });

                            context.Add (livre);
                            context.SaveChanges ();

                            newAuthors.Select (a => a.Id).ToList ().ForEach (x => {
                                context.Add (new Written { BookId = livre.Id, AuthorId = x });
                            });
                            context.SaveChanges ();
                        }

                    } catch (Exception e) {
                        Console.WriteLine (e.Message);
                    }

                });

            }

        }

    }
}