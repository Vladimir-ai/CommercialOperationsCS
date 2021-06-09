using Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace Core.EF
{
    public class MyContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<UserType> UserTypes { get; set; }
        public DbSet<Building> Buildings { get; set; }
        public DbSet<Street> Streets { get; set; }
        public DbSet<City> Cities { get; set; }
        public DbSet<Country> Countries { get; set; }
        public DbSet<Operation> Operations { get; set; }
        public DbSet<Item> Items { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<CategoryItem> CategoryItem { get; set; }
        
        public MyContext(DbContextOptions<MyContext> options):base(options)
        {
            //Database.EnsureDeleted();
            Database.EnsureCreated();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder
                .UseLazyLoadingProxies()
              //  .UseSqlServer("Server=localhost\\SQLEXPRESS;Database=mydb;Trusted_Connection=True;")
                ;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Country>().HasAlternateKey(c => new { c.Name });
            modelBuilder.Entity<City>().HasAlternateKey(city => new { city.CountryId, city.Name });
            modelBuilder.Entity<Street>().HasAlternateKey(st => new { st.CityId, st.Name });
            modelBuilder.Entity<Building>().HasAlternateKey(b => new { b.StreetId, b.Name });

            modelBuilder.Entity<Category>().HasIndex(i => i.Name).IsUnique();
            modelBuilder.Entity<UserType>().HasIndex(i => i.Type).IsUnique();

            modelBuilder.Entity<Item>()
                .HasMany(item => item.Categories)
                .WithMany(cat => cat.Items)
                .UsingEntity<CategoryItem>(
                    i => i
                        .HasOne(catit => catit.Category)
                        .WithMany(cat => cat.CategoryItem)
                        .HasForeignKey(catit => catit.CategoryId),
                    i => i
                        .HasOne(catit => catit.Item)
                        .WithMany(item => item.CategoryItem)
                        .HasForeignKey(catit => catit.ItemId),
                    i => i.HasKey(t => new { t.CategoryId, t.ItemId})
                );

            modelBuilder.Entity<Operation>()
                .HasOne(op => op.BuyingUser)
                .WithMany(t => t.BuyingOperations)
                .HasForeignKey(t => t.BuyingUserId)
                .OnDelete(DeleteBehavior.ClientCascade);

            modelBuilder.Entity<Operation>()
                .HasOne(op => op.SellingUser)
                .WithMany(t => t.SellingOperations)
                .HasForeignKey(t => t.SellingUserId)
                .OnDelete(DeleteBehavior.ClientCascade);
        }
    }
}
