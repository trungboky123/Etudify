using back_end.Entity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace back_end.Database;

public class AppDbContext : IdentityDbContext<User>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
        
    }
    
    public DbSet<RefreshToken> RefreshTokens { get; set; }
    public DbSet<Course> Courses { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Payment> Payments { get; set; }
    
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Course>(e =>
        {
            e.ToTable("course");
            e.HasKey(c => c.Id);
            e.Property(c => c.Id)
                .HasColumnName("id")
                .ValueGeneratedOnAdd();
            e.Property(c => c.Name)
                .HasMaxLength(50);
            e.Property(c => c.ThumbnailUrl)
                .HasColumnType("TEXT");
            e.Property(c => c.ListedPrice);
            e.Property(c => c.SalePrice);
            e.Property(c => c.Duration);
            e.Property(c => c.Description)
                .HasColumnType("TEXT");
            e.Property(c => c.Status);
            e.HasOne(c => c.Instructor)
                .WithMany(i => i.Courses)
                .HasForeignKey(c => c.InstructorId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasMany(c => c.Categories)
                .WithMany(c => c.Courses)
                .UsingEntity<Dictionary<string, object>>(
                    "course_category",
                    j => j.HasOne<Category>().WithMany().HasForeignKey("CategoryId"),
                    j => j.HasOne<Course>().WithMany().HasForeignKey("CourseId"),
                    j =>
                    {
                        j.HasKey("CategoryId", "CourseId");
                    });
        });

        builder.Entity<Category>(e =>
        {
            e.ToTable("category");
            e.HasKey(c => c.Id);
            e.Property(c => c.Id)
                .ValueGeneratedOnAdd();
            e.Property(c => c.Name)
                .HasMaxLength(50);
        });

        builder.Entity<RefreshToken>(e =>
        {
            e.ToTable("refresh_token");
            e.HasKey(rt => rt.Id);
            e.Property(rt => rt.Id)
                .ValueGeneratedOnAdd();
            e.Property(rt => rt.Token)
                .IsRequired();
            e.HasIndex(rt => rt.Token)
                .IsUnique();
            e.HasOne(rt => rt.User)
                .WithMany(u => u.RefreshTokens)
                .HasForeignKey(rt => rt.UserId);
            e.Property(rt => rt.ExpiredDate);
            e.Property(rt => rt.IsRevoked);
        });

        builder.Entity<Payment>(e =>
        {
            e.ToTable("payment");
            e.HasKey(p => p.Id);
            e.Property(p => p.Id)
                .ValueGeneratedOnAdd();
            e.Property(p => p.OrderCode)
                .IsRequired();
            e.HasIndex(p => p.OrderCode)
                .IsUnique();
            e.Property(p => p.Amount)
                .IsRequired();
            e.Property(p => p.Method)
                .IsRequired();
            e.Property(p => p.Status)
                .IsRequired();
            e.HasOne(p => p.User)
                .WithMany(u => u.Payments)
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(p => p.Course)
                .WithMany(c => c.Payments)
                .HasForeignKey(p => p.CourseId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}