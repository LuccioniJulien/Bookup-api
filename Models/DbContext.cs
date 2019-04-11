using System;
using BaseApi.Helper;
using Microsoft.AspNetCore.Hosting.Internal;
using Microsoft.EntityFrameworkCore;

namespace BaseApi.Models {
    public class DBcontext : DbContext {
        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating (ModelBuilder builder) {
            builder.Entity<User> ()
                .Property (u => u.PasswordHash)
                .IsRequired ();
        }

        protected override void OnConfiguring (DbContextOptionsBuilder optionsBuilder) {
            var (dbHost, dbName, dbUser, dbPassword) = new DbConfig ();
            optionsBuilder.UseNpgsql ($"Host={dbHost};Database={dbName};Username={dbUser};Password={dbPassword}");
        }
    }
}