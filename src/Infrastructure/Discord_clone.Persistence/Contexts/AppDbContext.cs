using Discord_clone.Domain.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Discord_clone.Persistence.Contexts;

public class AppDbContext : IdentityDbContext<AppUser>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Server> Servers { get; set; }
    public DbSet<Channel> Channels { get; set; }
    public DbSet<Message> Messages { get; set; }
    public DbSet<ServerMember> ServerMembers { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // N:N ƏLAQƏSİNİN QURULMASI

        // 1. Körpünün unikal açarı hər iki ID-nin birləşməsidir (Bir adam eyni serverə 2 dəfə qoşula bilməz)
        builder.Entity<ServerMember>()
            .HasKey(sm => new { sm.AppUserId, sm.ServerId });

        // 2. İstifadəçi ilə Körpü arasındakı əlaqə
        builder.Entity<ServerMember>()
            .HasOne(sm => sm.AppUser)
            .WithMany(u => u.ServerMembers)
            .HasForeignKey(sm => sm.AppUserId)
            .OnDelete(DeleteBehavior.NoAction); // XƏTA OLMAMASI ÜÇÜN NoAction edirik

        // 3. Server ilə Körpü arasındakı əlaqə
        builder.Entity<ServerMember>()
            .HasOne(sm => sm.Server)
            .WithMany(s => s.Members)
            .HasForeignKey(sm => sm.ServerId)
            .OnDelete(DeleteBehavior.Cascade); // Server silinəndə üzvlük də silinsin
    }
}