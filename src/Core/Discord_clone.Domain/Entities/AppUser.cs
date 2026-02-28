using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Discord_clone.Domain.Entities
{
    public class AppUser:IdentityUser
    {
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string? ProfileImageUrl { get; set; }
        public ICollection<Server> OwnedServers { get; set; } = new List<Server>();
        public ICollection<ServerMember> ServerMembers { get; set; } = new List<ServerMember>();
        public ICollection<Message> Messages { get; set; } = new List<Message>();

        // Dostluq əlaqələri
        // Mənim göndərdiyim dostluq istəkləri
        public ICollection<Friendship> SentFriendRequests { get; set; } = new List<Friendship>();

        // Mənə gələn dostluq istəkləri
        public ICollection<Friendship> ReceivedFriendRequests { get; set; } = new List<Friendship>();

        // Şəxsi Mesajlar (DM)
        public ICollection<DirectMessage> SentDirectMessages { get; set; } = new List<DirectMessage>();
        public ICollection<DirectMessage> ReceivedDirectMessages { get; set; } = new List<DirectMessage>();
    }
}
