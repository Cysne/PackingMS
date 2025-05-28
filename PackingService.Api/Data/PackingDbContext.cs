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

        public PackingDbContext(DbContextOptions<PackingDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<BoxEntity>().HasKey(b => b.Id);

            modelBuilder.Entity<ProductEntity>().HasKey(p => p.Id);

            modelBuilder.Entity<OrderEntity>().HasKey(o => o.Id);
            modelBuilder.Entity<OrderEntity>()
                .HasMany(o => o.OrderItems)
                .WithOne(oi => oi.Order)
                .HasForeignKey(oi => oi.OrderId);
            modelBuilder.Entity<OrderEntity>()
                .HasMany(o => o.OrderBoxes)
                .WithOne(ob => ob.Order)
                .HasForeignKey(ob => ob.OrderId);

            modelBuilder.Entity<OrderItemEntity>().HasKey(oi => oi.Id);
            modelBuilder.Entity<OrderItemEntity>()
                .HasOne(oi => oi.Product)
                .WithMany()
                .HasForeignKey(oi => oi.ProductId);

            modelBuilder.Entity<OrderBoxEntity>().HasKey(ob => ob.Id);
            modelBuilder.Entity<OrderBoxEntity>()
                .HasOne(ob => ob.Order)
                .WithMany(o => o.OrderBoxes)
                .HasForeignKey(ob => ob.OrderId);
            modelBuilder.Entity<OrderBoxEntity>()
                .HasOne(ob => ob.Box)
                .WithMany(b => b.OrderBoxes)
                .HasForeignKey(ob => ob.BoxId);
        }
    }
}