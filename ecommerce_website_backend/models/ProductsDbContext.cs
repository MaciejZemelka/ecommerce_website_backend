using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace ecommerce_website_backend.models
{
    public class ProductsDbContext : DbContext
    {
        public ProductsDbContext(DbContextOptions<ProductsDbContext> options) : base(options) { }

        // Tabele w bazie danych
        public DbSet<Product> Products { get; set; }
        public DbSet<ProductVariant> ProductVariants { get; set; }
        public DbSet<ProductImages> ProductImages { get; set; }
        public DbSet<ShoeSize> ShoeSizes { get; set; }
        public DbSet<VariantSize> VariantSizes { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Konfiguracja klucza głównego dla Product
            modelBuilder.Entity<Product>()
                 .HasKey(p => p.ProductId);

            // Konfiguracja klucza głównego dla ProductVariant
            modelBuilder.Entity<ProductVariant>()
                .HasKey(v => v.VariantId);

            // Relacja: ProductVariant -> Product
            modelBuilder.Entity<ProductVariant>()
                .HasOne(v => v.Product)
                .WithMany(p => p.ProductVariants)
                .HasForeignKey(v => v.ProductId);

            // Konfiguracja klucza głównego dla ProductImages
            modelBuilder.Entity<ProductImages>()
                .HasKey(pi => pi.ImagesId);

            // Relacja: ProductImages -> ProductVariant
            modelBuilder.Entity<ProductImages>()
                .HasOne(pi => pi.ProductVariant)
                .WithMany(v => v.ProductImages)
                .HasForeignKey(pi => pi.VariantId);

            // Konfiguracja klucza głównego dla ShoeSize
            modelBuilder.Entity<ShoeSize>()
                .HasKey(s => s.SizeId);

            // Konfiguracja klucza głównego dla VariantSize
            modelBuilder.Entity<VariantSize>()
                .HasKey(vs => new { vs.VariantId, vs.SizeId }); // Klucz złożony

            // Relacja: VariantSize -> ProductVariant
            modelBuilder.Entity<VariantSize>()
                .HasOne(vs => vs.ProductVariant)
                .WithMany(v => v.VariantSizes)
                .HasForeignKey(vs => vs.VariantId);

            // Relacja: VariantSize -> ShoeSize
            modelBuilder.Entity<VariantSize>()
                .HasOne(vs => vs.ShoeSize)
                .WithMany(s => s.VariantSizes)
                .HasForeignKey(vs => vs.SizeId);
        }
    }

    // Modele
    public class Product
    {
        public int ProductId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }
        public float Price { get; set; }
        public string Gender { get; set; }
        public string Height { get; set; }
        public ICollection<ProductVariant> ProductVariants { get; set; }
    }

    public class ProductVariant
    {
        public int VariantId { get; set; }
        public int ProductId { get; set; }
        public string Color { get; set; }
        public int Discount { get; set; }
        public Product Product { get; set; }
        public ICollection<ProductImages> ProductImages { get; set; }
        public ICollection<VariantSize> VariantSizes { get; set; }
    }

    public class ProductImages
    {
        public int ImagesId { get; set; }
        public int VariantId { get; set; }
        public string Image1 { get; set; }
        public string Image2 { get; set; }
        public string Image3 { get; set; }
        public string Image4 { get; set; }
        public ProductVariant ProductVariant { get; set; }
    }

    public class ShoeSize
    {
        public int SizeId { get; set; }
        public int Size { get; set; }
        public ICollection<VariantSize> VariantSizes { get; set; }
    }

    public class VariantSize
    {
        public int VariantId { get; set; }
        public int SizeId { get; set; }
        public int Stock { get; set; }
        public ProductVariant ProductVariant { get; set; }
        public ShoeSize ShoeSize { get; set; }
    }
}
