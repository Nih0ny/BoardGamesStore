using BoardGamesStore.Models.Entities;
using BoardGamesStore.Models.Enums;
using BoardGamesStore.Models.Views;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;

namespace BoardGamesStore.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<User, IdentityRole, string, IdentityUserClaim<string>, IdentityUserRole<string>, IdentityUserLogin<string>, IdentityRoleClaim<string>, IdentityUserToken<string>>(options)
{
    public DbSet<Product> Products { get; set; }
    public DbSet<Comment> Comments { get; set; }
    public DbSet<CartItem> CartItems { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderStatus> OrderStatuses { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }
    public DbSet<PaymentTransaction> PaymentTransactions { get; set; }
    public DbSet<BonusTransaction> BonusTransactions { get; set; }
    public DbSet<SimilarProduct> SimilarProducts { get; set; }
    public DbSet<WishlistItem> WishlistItems { get; set; }
    public DbSet<Evaluation> Evaluations { get; set; }
    public DbSet<ProductReport> ProductReports { get; set; }
    public DbSet<CommentReport> CommentReports { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }
    public DbSet<ReportStatus> ReportStatuses { get; set; }
    public DbSet<PaymentStatus> PaymentStatuses { get; set; }
    public DbSet<ProductRatingSummary> ProductRatingSummaries { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<DeliveryMethod> DeliveryMethods { get; set; }
    public DbSet<ProductCategory> ProductCategories { get; set; }
    public DbSet<ProductImage> ProductImages { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Product entity
        modelBuilder.Entity<Product>(entity =>
        {
            entity.ToTable("products");
            entity.HasKey(p => p.Id);
            entity.Property(p => p.Name).IsRequired().HasMaxLength(255);
            entity.Property(p => p.Price).HasColumnType("decimal(18,2)");
            entity.Property(p => p.BonusRate).HasColumnType("decimal(5,2)");
            entity.Property(p => p.MaxBonusPaymentPercent).HasColumnType("decimal(5,2)");
        });

        // ProductCategory entity
        modelBuilder.Entity<ProductCategory>(entity =>
        {
            entity.ToTable("product_categories");
            entity.HasKey(pc => new { pc.ProductId, pc.CategoryId });
            entity.HasOne(pc => pc.Product)
                .WithMany(p => p.Categories)
                .HasForeignKey(pc => pc.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(pc => pc.Category)
                .WithMany(c => c.Products)
                .HasForeignKey(pc => pc.CategoryId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ProductRatingSummary view
        modelBuilder.Entity<ProductRatingSummary>(entity =>
        {
            entity.ToView("product_stats_mv");
            entity.HasKey(ps => ps.ProductId);
            entity.HasOne(p => p.Product)
                .WithOne(ps => ps.RatingSummary)
                .HasForeignKey<ProductRatingSummary>(ps => ps.ProductId);
        });

        // Comment entity
        modelBuilder.Entity<Comment>(entity =>
        {
            entity.ToTable("comments");
            entity.HasKey(c => c.Id);
            entity.HasOne(c => c.User)
                .WithMany(u => u.Comments)
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(c => c.Product)
                .WithMany(p => p.Comments)
                .HasForeignKey(c => c.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // CartItems entity (1 user - many cart items)
        modelBuilder.Entity<CartItem>(entity =>
        {
            entity.ToTable("cart_items");
            entity.HasKey(ci => ci.Id);
            entity.HasOne(ci => ci.User)
                .WithMany(c => c.CartItems)
                .HasForeignKey(ci => ci.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(ci => ci.Product)
                .WithMany(p => p.CartItems)
                .HasForeignKey(ci => ci.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Order entity
        modelBuilder.Entity<Order>(entity =>
        {
            entity.ToTable("orders");
            entity.HasKey(o => o.Id);
            entity.Property(o => o.Total).HasColumnType("decimal(18,2)");
            entity.HasOne(o => o.User)
                .WithMany(u => u.Orders)
                .HasForeignKey(o => o.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(o => o.Status)
                .WithMany(os => os.Orders)
                .HasForeignKey(o => o.StatusId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(o => o.DeliveryMethod)
                .WithMany(dm => dm.Orders)
                .HasForeignKey(o => o.DeliveryMethodId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(o => o.PaymentStatus)
                .WithMany(ps => ps.Orders)
                .HasForeignKey(o => o.PaymentStatusId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // OrderItem entity
        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.ToTable("order_items");
            entity.HasKey(oi => oi.Id);
            entity.Property(oi => oi.Price).HasColumnType("decimal(18,2)");
            entity.HasOne(oi => oi.Order)
                .WithMany(o => o.OrderItems)
                .HasForeignKey(oi => oi.OrderId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(oi => oi.Product)
                .WithMany(p => p.OrderItems)
                .HasForeignKey(oi => oi.ProductId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // PaymentTransaction entity
        modelBuilder.Entity<PaymentTransaction>(entity =>
        {
            entity.ToTable("payment_transactions");
            entity.HasKey(pt => pt.Id);
            entity.Property(pt => pt.Amount).HasColumnType("decimal(18,2)");
            entity.HasOne(pt => pt.Order)
                .WithMany(o => o.PaymentTransactions)
                .HasForeignKey(pt => pt.OrderId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // BonusTransaction entity
        modelBuilder.Entity<BonusTransaction>(entity =>
        {
            entity.ToTable("bonus_transactions");
            entity.HasKey(bt => bt.Id);
            entity.Property(bt => bt.Amount).HasColumnType("decimal(18,2)");
            entity.HasOne(bt => bt.User)
                .WithMany(u => u.BonusTransactions)
                .HasForeignKey(bt => bt.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(bt => bt.Order)
                .WithMany(o => o.BonusTransactions)
                .HasForeignKey(bt => bt.OrderId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // SimilarProduct entity
        modelBuilder.Entity<SimilarProduct>(entity =>
        {
            entity.ToTable("similar_products");
            entity.HasKey(sp => new { sp.ProductId, sp.SimilarProductId });
            entity.HasOne(sp => sp.Product)
                .WithMany(p => p.SimilarProducts)
                .HasForeignKey(sp => sp.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(sp => sp.SimilarTo)
                .WithMany(p => p.RelatedToProducts)
                .HasForeignKey(sp => sp.SimilarProductId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // WishlistItems entity (1 user - many wishlist items)
        modelBuilder.Entity<WishlistItem>(entity =>
        {
            entity.ToTable("wishlists");
            entity.HasKey(w => new { w.UserId, w.ProductId });
            entity.HasOne(w => w.User)
                .WithMany(u => u.WishlistItems)
                .HasForeignKey(w => w.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(w => w.Product)
                .WithMany(p => p.WishlistItems)
                .HasForeignKey(w => w.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Evaluation entity (1 user - many evaluations)
        modelBuilder.Entity<Evaluation>(entity =>
        {
            entity.ToTable("evaluations");
            entity.HasKey(e => new { e.UserId, e.ProductId });
            entity.HasOne(e => e.User)
                .WithMany(u => u.Evaluations)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Product)
                .WithMany(p => p.Evaluations)
                .HasForeignKey(e => e.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ProductReport entity
        modelBuilder.Entity<ProductReport>(entity =>
        {
            entity.ToTable("product_reports");
            entity.HasKey(pr => pr.Id);
            entity.HasOne(pr => pr.User)
                .WithMany(u => u.ProductReports)
                .HasForeignKey(pr => pr.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(pr => pr.Product)
                .WithMany(p => p.ProductReports)
                .HasForeignKey(pr => pr.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(pr => pr.Status)
                .WithMany(s => s.ProductReports)
                .HasForeignKey(pr => pr.StatusId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // CommentReport entity
        modelBuilder.Entity<CommentReport>(entity =>
        {
            entity.ToTable("comment_reports");
            entity.HasKey(cr => cr.Id);
            entity.HasOne(cr => cr.User)
                .WithMany(u => u.CommentReports)
                .HasForeignKey(cr => cr.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(cr => cr.Comment)
                .WithMany(c => c.CommentReports)
                .HasForeignKey(cr => cr.CommentId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(cr => cr.Status)
                .WithMany(s => s.CommentReports)
                .HasForeignKey(cr => cr.StatusId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // RefreshToken entity
        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.ToTable("refresh_tokens");
            entity.HasKey(rt => rt.Id);
            entity.HasOne(rt => rt.User)
                .WithMany(u => u.RefreshTokens)
                .HasForeignKey(rt => rt.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(rt => rt.Token).IsUnique();
        });

        // User entity
        modelBuilder.Entity<User>(entity =>
        {
            entity.Property(u => u.Coins).HasColumnType("decimal(18,2)");
        });

        // Category entity
        modelBuilder.Entity<Category>(entity =>
        {
            entity.ToTable("categories");
            entity.HasKey(c => c.Id);
            entity.Property(c => c.Name).IsRequired().HasMaxLength(100);
        });

        // ProductImage entity
        modelBuilder.Entity<ProductImage>(entity =>
        {
            entity.ToTable("product_images");
            entity.HasKey(pi => pi.Id);
            entity.Property(pi => pi.FileName).IsRequired().HasMaxLength(255);
            entity.Property(pi => pi.RelativePath).IsRequired().HasMaxLength(500);
            entity.HasOne(pi => pi.Product)
                .WithMany(p => p.Images)
                .HasForeignKey(pi => pi.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(pi => new { pi.ProductId, pi.DisplayOrder });
        });

        // STATUSES

        // ReportStatus entity
        modelBuilder.Entity<ReportStatus>(entity =>
        {
            entity.ToTable("report_statuses");
            entity.Property(s => s.Id)
                .HasConversion<int>()
                .ValueGeneratedNever();
            entity.HasData(
                Enum.GetValues<ReportStatusId>()
                    .Select(e => new ReportStatus
                    {
                        Id = e,
                        Name = e.ToString()
                    })
            );
        });

        // PaymentStatus entity
        modelBuilder.Entity<PaymentStatus>(entity =>
        {
            entity.ToTable("payment_statuses");
            entity.Property(s => s.Id)
                .HasConversion<int>()
                .ValueGeneratedNever();
            entity.HasData(
                Enum.GetValues<PaymentStatusId>()
                    .Select(e => new PaymentStatus
                    {
                        Id = e,
                        Name = e.ToString()
                    })
            );
        });

        // OrderStatus entity
        modelBuilder.Entity<OrderStatus>(entity =>
        {
            entity.ToTable("order_statuses");
            entity.Property(s => s.Id)
                .HasConversion<int>()
                .ValueGeneratedNever();
            entity.HasData(
                Enum.GetValues<OrderStatusId>()
                    .Select(e => new OrderStatus
                    {
                        Id = e,
                        Name = e.ToString()
                    })
            );
        });

        // DeliveryMethod entity
        modelBuilder.Entity<DeliveryMethod>(entity =>
        {
            entity.ToTable("delivery_methods");
            entity.Property(s => s.Id)
                .HasConversion<int>()
                .ValueGeneratedNever();
            entity.HasData(
                Enum.GetValues<DeliveryMethodId>()
                    .Select(e => new DeliveryMethod
                    {
                        Id = e,
                        Name = e.ToString()
                    })
            );
        });
    }
}
