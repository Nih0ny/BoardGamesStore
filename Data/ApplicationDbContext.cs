using BoardGamesStore.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Npgsql.EntityFrameworkCore.PostgreSQL;

namespace BoardGamesStore.Data;

public class ApplicationDbContext : IdentityDbContext<User, Role, int>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Product> Products { get; set; }
    public DbSet<Comment> Comments { get; set; }
    public DbSet<Cart> Carts { get; set; }
    public DbSet<CartItem> CartItems { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderStatus> OrderStatuses { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }
    public DbSet<PaymentTransaction> PaymentTransactions { get; set; }
    public DbSet<BonusTransaction> BonusTransactions { get; set; }
    public DbSet<SimilarProduct> SimilarProducts { get; set; }
    public DbSet<Wishlist> Wishlists { get; set; }
    public DbSet<Evaluation> Evaluations { get; set; }
    public DbSet<ProductReport> ProductReports { get; set; }
    public DbSet<CommentReport> CommentReports { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("board_games_store");
        base.OnModelCreating(modelBuilder);

        // User ↔ Role (one-to-many)
        modelBuilder.Entity<User>()
            .HasOne(u => u.Role)
            .WithMany(r => r.Users)
            .HasForeignKey(u => u.RoleId)
            .OnDelete(DeleteBehavior.Restrict);

        // Comment ↔ User & Product
        modelBuilder.Entity<Comment>()
            .HasOne(c => c.User)
            .WithMany(u => u.Comments)
            .HasForeignKey(c => c.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Comment>()
            .HasOne(c => c.Product)
            .WithMany(p => p.Comments)
            .HasForeignKey(c => c.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        // Cart ↔ User
        modelBuilder.Entity<Cart>()
            .HasOne(c => c.User)
            .WithMany(u => u.Carts)
            .HasForeignKey(c => c.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // CartItem ↔ Cart & Product
        modelBuilder.Entity<CartItem>()
            .HasOne(ci => ci.Cart)
            .WithMany(c => c.Items)
            .HasForeignKey(ci => ci.CartId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<CartItem>()
            .HasOne(ci => ci.Product)
            .WithMany(p => p.CartItems)
            .HasForeignKey(ci => ci.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        // Order ↔ User & Status
        modelBuilder.Entity<Order>()
            .HasOne(o => o.User)
            .WithMany(u => u.Orders)
            .HasForeignKey(o => o.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Order>()
            .HasOne(o => o.Status)
            .WithMany(s => s.Orders)
            .HasForeignKey(o => o.StatusId)
            .OnDelete(DeleteBehavior.Restrict);

        // OrderItem ↔ Order & Product
        modelBuilder.Entity<OrderItem>()
            .HasOne(oi => oi.Order)
            .WithMany(o => o.OrderItems)
            .HasForeignKey(oi => oi.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<OrderItem>()
            .HasOne(oi => oi.Product)
            .WithMany(p => p.OrderItems)
            .HasForeignKey(oi => oi.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        // PaymentTransaction ↔ Order
        modelBuilder.Entity<PaymentTransaction>()
            .HasOne(pt => pt.Order)
            .WithMany(o => o.PaymentTransactions)
            .HasForeignKey(pt => pt.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        // BonusTransaction ↔ User & Order
        modelBuilder.Entity<BonusTransaction>()
            .HasOne(bt => bt.User)
            .WithMany(u => u.BonusTransactions)
            .HasForeignKey(bt => bt.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<BonusTransaction>()
            .HasOne(bt => bt.Order)
            .WithMany(o => o.BonusTransactions)
            .HasForeignKey(bt => bt.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        // SimilarProduct (self-referencing)
        modelBuilder.Entity<SimilarProduct>()
            .HasOne(sp => sp.Product)
            .WithMany(p => p.SimilarProducts)
            .HasForeignKey(sp => sp.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<SimilarProduct>()
            .HasOne(sp => sp.SimilarTo)
            .WithMany(p => p.RelatedToProducts)
            .HasForeignKey(sp => sp.SimilarProductId)
            .OnDelete(DeleteBehavior.Cascade);

        // Wishlist ↔ User & Product
        modelBuilder.Entity<Wishlist>()
            .HasOne(w => w.User)
            .WithMany(u => u.Wishlists)
            .HasForeignKey(w => w.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Wishlist>()
            .HasOne(w => w.Product)
            .WithMany(p => p.Wishlists)
            .HasForeignKey(w => w.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        // Evaluation ↔ User & Product
        modelBuilder.Entity<Evaluation>()
            .HasOne(e => e.User)
            .WithMany(u => u.Evaluations)
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Evaluation>()
            .HasOne(e => e.Product)
            .WithMany(p => p.Evaluations)
            .HasForeignKey(e => e.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        // ProductReport ↔ User & Product
        modelBuilder.Entity<ProductReport>()
            .HasOne(pr => pr.User)
            .WithMany(u => u.ProductReports)
            .HasForeignKey(pr => pr.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ProductReport>()
            .HasOne(pr => pr.Product)
            .WithMany(p => p.ProductReports)
            .HasForeignKey(pr => pr.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        // CommentReport ↔ User & Comment
        modelBuilder.Entity<CommentReport>()
            .HasOne(cr => cr.User)
            .WithMany(u => u.CommentReports)
            .HasForeignKey(cr => cr.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<CommentReport>()
            .HasOne(cr => cr.Comment)
            .WithMany(c => c.CommentReports)
            .HasForeignKey(cr => cr.CommentId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.HasAnnotation(
            "Relational:HistoryTableSchema", "board_games_store"
        );
    }
}
