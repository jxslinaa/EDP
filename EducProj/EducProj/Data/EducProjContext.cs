using System;
using EducProj.Controllers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using EducProj.Models;

namespace EducProj.Models
{
    public partial class EducProjContext : DbContext
    {
        public EducProjContext()
        {
        }

        public EducProjContext(DbContextOptions<EducProjContext> options)
            : base(options)
        {
           
        }
        //this function used for bulk operation
        //to implement this function in your destination 
        public int executeSql(string sql) {
            return base.Database.ExecuteSqlCommand(sql);
        }
        public static string connString { get; set; }

        public virtual DbSet<Workshopitems> Workshopitems { get; set; }
        public virtual DbSet<ShoppingCartItem> ShoppingCartItem { get; set; }
        public virtual DbSet<Users> Users { get; set; }

        public virtual DbSet<Orders> Orders { get; set; }
        public virtual DbSet<OrderItems> OrderItems { get; set; }
        public virtual DbSet<WorkShopItemImages> WorkShopItemImages { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer(connString);
            }
            //optionsBuilder.UseSqlServer("Data Source=DESKTOP-95EL5EC\\SQLEXPRESS;Initial Catalog=CanERP;integrated security=True;MultipleActiveResultSets=True;Database=CanERP;Trusted_Connection=True;");
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Users>(entity =>
            {
                entity.ToTable("Users");
                entity.Property(e => e.UserId).HasColumnName("UserId");
            });
            modelBuilder.Entity<Workshopitems>(entity =>
            {
                entity.ToTable("Workshopitems");
                entity.Property(e => e.SessionId).HasColumnName("SessionId");
            });
            modelBuilder.Entity<ShoppingCartItem>(entity =>
            {
                entity.ToTable("ShoppingCartItem");
                entity.Property(e => e.CartItemId).HasColumnName("CartItemId");
            });

           modelBuilder.Entity<Orders>(entity =>
            {
                entity.ToTable("Orders");
                entity.Property(e => e.CartId).HasColumnName("CartId");
            }); 
          
            modelBuilder.Entity<OrderItems>(entity =>
            {
                entity.ToTable("OrderItems");
                entity.Property(e => e.OrderItemId).HasColumnName("OrderItemId");
            });

        }
        public DbSet<EducProj.Models.BookingDetails> BookingDetails { get; set; }
    }
}
