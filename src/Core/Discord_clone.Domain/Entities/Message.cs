using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Discord_clone.Domain.Entities
{
    public class Message:BaseEntity
    {
        public string Content { get; set; } = string.Empty;
        public DateTime SentAt { get; set; } = DateTime.UtcNow;

        // 1. KİM göndərib? (AppUser ilə 1:N əlaqə)
        public string SenderId { get; set; } = string.Empty;
        public AppUser? Sender { get; set; }

        // 2. HANSI kanala göndərilib? (Channel ilə 1:N əlaqə)
        public Guid ChannelId { get; set; }
        public Channel? Channel { get; set; }
        public bool IsEdited { get; set; } = false; // Dəyişdirilibmi?
        public DateTime? UpdatedAt { get; set; }    // Nə vaxt dəyişdirilib?
    }
}
