using EduHub.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EduHub.Infrastructure.Persistence;

public class EduHubDbContext(DbContextOptions<EduHubDbContext> options) : DbContext(options)
{
    public DbSet<Member> Members => Set<Member>();
    public DbSet<Club> Clubs => Set<Club>();
    public DbSet<Event> Events => Set<Event>();
    public DbSet<Registration> Registrations => Set<Registration>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Member>(b =>
        {
            b.ToTable("members");
            b.HasKey(x => x.Id);
            b.Property(x => x.StudentId).HasMaxLength(20).IsRequired();
            b.Property(x => x.FullName).HasMaxLength(150).IsRequired();
            b.Property(x => x.Email).HasMaxLength(200).IsRequired();
            b.HasIndex(x => x.StudentId).IsUnique();
            b.HasIndex(x => x.Email).IsUnique();          // Email duy nhất
        });

        modelBuilder.Entity<Club>(b =>
        {
            b.ToTable("clubs");
            b.HasKey(x => x.Id);
            b.Property(x => x.Name).HasMaxLength(150).IsRequired();
            b.Property(x => x.Description).HasMaxLength(1000);
        });

        modelBuilder.Entity<Event>(b =>
        {
            b.ToTable("events");
            b.HasKey(x => x.Id);
            b.Property(x => x.Name).HasMaxLength(200).IsRequired();
            b.Property(x => x.Description).HasMaxLength(2000);
            b.Property(x => x.Location).HasMaxLength(200);
            b.Property(x => x.Capacity).HasDefaultValue(100); // Capacity mặc định 100
            b.HasOne<Club>().WithMany().HasForeignKey(x => x.ClubId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Registration>(b =>
        {
            b.ToTable("registrations");
            b.HasKey(x => x.Id);
            b.Property(x => x.Status).HasMaxLength(20).IsRequired();
            b.HasOne<Member>().WithMany().HasForeignKey(x => x.MemberId)
                .OnDelete(DeleteBehavior.Cascade);
            b.HasOne<Event>().WithMany().HasForeignKey(x => x.EventId)
                .OnDelete(DeleteBehavior.Cascade);
            b.HasIndex(x => new { x.MemberId, x.EventId }).IsUnique(); // mỗi member đăng ký 1 lần/event
        });
    }

    // Seed: 3 clubs, 2 events (+ vài member để demo đăng ký). Chỉ chạy khi DB còn trống.
    public async Task SeedAsync(CancellationToken ct = default)
    {
        if (await Clubs.AnyAsync(ct)) return;

        var clubs = new[]
        {
            Club.Create("CLB Lập trình", "Sinh hoạt lập trình, thi đấu thuật toán"),
            Club.Create("CLB Trí tuệ nhân tạo", "Học máy, dữ liệu và ứng dụng AI"),
            Club.Create("CLB An toàn thông tin", "CTF, bảo mật ứng dụng web"),
        };
        Clubs.AddRange(clubs);

        Events.AddRange(
            Event.Create("Workshop .NET 10", clubs[0].Id, DateTime.UtcNow.AddDays(14), 100,
                "Làm quen EF Core và PostgreSQL", "Phòng B101"),
            Event.Create("Hackathon nhỏ", clubs[1].Id, DateTime.UtcNow.AddDays(30), 2,
                "Thi nhanh trong một buổi", "Phòng B102")); // 2 chỗ để demo "đầy"

        Members.AddRange(
            Member.Create("SV001", "Nguyễn Văn An", "an@dthu.edu.vn"),
            Member.Create("SV002", "Trần Thị Bình", "binh@dthu.edu.vn"),
            Member.Create("SV003", "Lê Minh Chi", "chi@dthu.edu.vn"));

        await SaveChangesAsync(ct);
    }
}
