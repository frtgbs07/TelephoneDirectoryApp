using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using TelephoneDirectory.UserService.DataAccessLayer.Entities;

namespace TelephoneDirectory.UserService.DataAccessLayer.USContext
{
   public class UserServiceContext: DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Contact> Contacts { get; set; }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseNpgsql("Server=localhost;Port=5432;Database=UserServiceDb;Integrated Security=true; User Id=postgres;Password=123;");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>().ToTable("User", "public");
            modelBuilder.Entity<Contact>().ToTable("Contact", "public");
        }
    }
}
