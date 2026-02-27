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

        // --- N:N ƏLAQƏSİNİN QURULMASI (ServerMembers) ---

        builder.Entity<ServerMember>()
            .HasKey(sm => new { sm.AppUserId, sm.ServerId });

        builder.Entity<ServerMember>()
            .HasOne(sm => sm.AppUser)
            .WithMany(u => u.ServerMembers)
            .HasForeignKey(sm => sm.AppUserId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.Entity<ServerMember>()
            .HasOne(sm => sm.Server)
            .WithMany(s => s.Members)
            .HasForeignKey(sm => sm.ServerId)
            .OnDelete(DeleteBehavior.Cascade);


        // --- YENİ: MESAJ ƏLAQƏLƏRİNİN QURULMASI ---

        builder.Entity<Message>()
            .HasOne(m => m.Sender)
            .WithMany(u => u.Messages)
            .HasForeignKey(m => m.SenderId)
            .OnDelete(DeleteBehavior.NoAction); // İstifadəçi silinəndə mesajlar silinməsin (xəta olmasın deyə)

        builder.Entity<Message>()
            .HasOne(m => m.Channel)
            .WithMany(c => c.Messages)
            .HasForeignKey(m => m.ChannelId)
            .OnDelete(DeleteBehavior.Cascade); // Kanal silinəndə içindəki mesajlar da uçsun
    }
}