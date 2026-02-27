using Discord_clone.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Discord_clone.Domain.Entities
{
    public class Friendship
    {
        // 1. İstəyi GÖNDƏRƏN tərəf
        public string RequesterId { get; set; } = string.Empty;
        public AppUser? Requester { get; set; }

        // 2. İstəyi ALAN tərəf
        public string ReceiverId { get; set; } = string.Empty;
        public AppUser? Receiver { get; set; }

        // Status (Gözləmədə, Qəbul edildi, Bloklandı)
        public FriendshipStatus Status { get; set; } = FriendshipStatus.Pending;

        // Nə vaxt istək göndərilib/dost olunub?
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
