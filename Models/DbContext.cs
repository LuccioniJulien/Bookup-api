using System;
using BaseApi.Helper;
using Microsoft.AspNetCore.Hosting.Internal;
using Microsoft.EntityFrameworkCore;

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

    }
}