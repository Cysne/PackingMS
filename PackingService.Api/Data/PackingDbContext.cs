using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.SqlServer;
using PackingService.Api.Entities;


namespace PackingService.Api.Data
{
    public class PackingDbContext : DbContext
    {
        public DbSet<BoxEntity> Boxes { get; set; }
        public DbSet<ProductEntity> Products { get; set; }
        public DbSet<OrderEntity> Orders { get; set; }
        public DbSet<OrderItemEntity> OrderItems { get; set; }
        public DbSet<OrderBoxEntity> OrderBoxes { get; set; }
        public DbSet<User> Users { get; set; }

        public PackingDbContext(DbContextOptions<PackingDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<BoxEntity>().HasKey(b => b.BoxId);
            modelBuilder.Entity<BoxEntity>()
                .Property(b => b.Height)
                .HasPrecision(18, 2);
            modelBuilder.Entity<BoxEntity>()
                .Property(b => b.Width)
                .HasPrecision(18, 2);
            modelBuilder.Entity<BoxEntity>()
                .Property(b => b.Length)
                .HasPrecision(18, 2);

            modelBuilder.Entity<ProductEntity>().HasKey(p => p.ProductId);
            modelBuilder.Entity<ProductEntity>()
                .Property(p => p.Height)
                .HasPrecision(18, 2);
            modelBuilder.Entity<ProductEntity>()
                .Property(p => p.Width)
                .HasPrecision(18, 2);
            modelBuilder.Entity<ProductEntity>()
                .Property(p => p.Length)
                .HasPrecision(18, 2);

            modelBuilder.Entity<OrderEntity>().HasKey(o => o.OrderId);
            modelBuilder.Entity<OrderEntity>()
                .HasMany(o => o.OrderItems)
                .WithOne(oi => oi.Order)
                .HasForeignKey(oi => oi.OrderId);
            modelBuilder.Entity<OrderEntity>()
                .HasMany(o => o.OrderBoxes)
                .WithOne(ob => ob.Order)
                .HasForeignKey(ob => ob.OrderId);
            modelBuilder.Entity<OrderEntity>()
                .HasOne(o => o.User)
                .WithMany()
                .HasForeignKey(o => o.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<OrderItemEntity>().HasKey(oi => oi.OrderItemId);
            modelBuilder.Entity<OrderItemEntity>()
                .HasOne(oi => oi.Product)
                .WithMany()
                .HasForeignKey(oi => oi.ProductId);

            modelBuilder.Entity<OrderBoxEntity>().HasKey(ob => new { ob.OrderId, ob.BoxId });
            modelBuilder.Entity<OrderBoxEntity>()
                .HasOne(ob => ob.Order)
                .WithMany(o => o.OrderBoxes)
                .HasForeignKey(ob => ob.OrderId);
            modelBuilder.Entity<OrderBoxEntity>()
                .HasOne(ob => ob.Box)
                .WithMany(b => b.OrderBoxes)
                .HasForeignKey(ob => ob.BoxId);

            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.UserId);
                entity.Property(e => e.Username).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
                entity.Property(e => e.PasswordHash).IsRequired();
                entity.HasIndex(e => e.Email).IsUnique();
                entity.HasIndex(e => e.Username).IsUnique();
            });
        }

        public void SeedData()
        {
            if (!Boxes.Any())
            {
                Boxes.AddRange(new[]
                {
                    new BoxEntity { BoxType = "Caixa 1", Height = 30m, Width = 40m, Length = 80m },
                    new BoxEntity { BoxType = "Caixa 2", Height = 80m, Width = 50m, Length = 40m },
                    new BoxEntity { BoxType = "Caixa 3", Height = 50m, Width = 80m, Length = 60m }
                });
                SaveChanges();
            }

            if (!Products.Any())
            {
                Products.AddRange(new[]
                {
                    new ProductEntity { Name = "Produto A", Height = 10m, Width = 10m, Length = 10m },
                    new ProductEntity { Name = "Produto B", Height = 20m, Width = 15m, Length = 5m }
                });
                SaveChanges();
            }

            if (!Users.Any())
            {
                var adminUser = new User
                {
                    Username = "admin",
                    Email = "admin@test.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!"),
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                };
                Users.Add(adminUser);
                SaveChanges();
                adminUser = Users.First(u => u.Username == "admin");
                if (!Orders.Any())
                {
                    var productA = Products.First();
                    var order = new OrderEntity
                    {
                        OrderDate = DateTime.UtcNow,
                        UserId = adminUser.UserId,
                        OrderItems = new List<OrderItemEntity>
                        {
                            new OrderItemEntity { ProductId = productA.ProductId, Quantity = 2 }
                        }
                    };
                    Orders.Add(order);
                    SaveChanges();
                }
            }
            else if (!Orders.Any())
            {
                var productA = Products.First();
                var adminUser = Users.First(u => u.Username == "admin");
                var order = new OrderEntity
                {
                    OrderDate = DateTime.UtcNow,
                    UserId = adminUser.UserId,
                    OrderItems = new List<OrderItemEntity>
                    {
                        new OrderItemEntity { ProductId = productA.ProductId, Quantity = 2 }
                    }
                };
                Orders.Add(order);
                SaveChanges();
            }
        }
    }
}