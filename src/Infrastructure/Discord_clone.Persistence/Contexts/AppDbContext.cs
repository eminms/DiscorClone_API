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
    public DbSet<Friendship> Friendships { get; set; }
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

        builder.Entity<Channel>()
            .HasOne(c => c.Server)
            .WithMany(s => s.Channels)
            .HasForeignKey(c => c.ServerId)
            .OnDelete(DeleteBehavior.Cascade); // Server silinəndə kanalları da silinsin

        // --- DOSTLUQ (FRIENDSHIP) ƏLAQƏLƏRİ ---

        // Unikal açar: İki adam arasında yalnız 1 qeyd ola bilər
        builder.Entity<Friendship>()
            .HasKey(f => new { f.RequesterId, f.ReceiverId });

        // İstəyi göndərən tərəf
        builder.Entity<Friendship>()
            .HasOne(f => f.Requester)
            .WithMany(u => u.SentFriendRequests)
            .HasForeignKey(f => f.RequesterId)
            .OnDelete(DeleteBehavior.Restrict); // DİQQƏT: Restrict yazırıq ki, SQL silinmə xətası verməsin

        // İstəyi alan tərəf
        builder.Entity<Friendship>()
            .HasOne(f => f.Receiver)
            .WithMany(u => u.ReceivedFriendRequests)
            .HasForeignKey(f => f.ReceiverId)
            .OnDelete(DeleteBehavior.Restrict); // Yenə Restrict
    }
}