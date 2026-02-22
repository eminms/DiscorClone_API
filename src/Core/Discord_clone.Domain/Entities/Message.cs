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

        // Mesajı kim yazıb? (AppUser Id)
        public string SenderId { get; set; } = string.Empty;

        // Mesaj hansı kanala yazılıb?
        public Guid ChannelId { get; set; }
    }
}
