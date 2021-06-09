using Microsoft.EntityFrameworkCore;
using System;

namespace App.Infrastructure.Data
{
    public class Context<T> where T : DbContext
    {
        public DbSet<T> Data { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseMySql("server=localhost; user=root; database=usersdb5;");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Building>().HasAlternateKey(b => new { b.StreetId, b.Name });
            modelBuilder.Entity<Street>().HasAlternateKey(st => new { st.CityId, st.Name });
            modelBuilder.Entity<City>().HasAlternateKey(city => new { city.CountryId, city.Name });
            modelBuilder.Entity<Country>().HasAlternateKey(c => new { c.Name });

            modelBuilder.Entity<ItemAndCategory>().HasKey(iac => new { iac.CategoryId, iac.ItemId });
            modelBuilder.Entity<Category>().HasIndex(i => i.Name).IsUnique();
        }
    }
}
