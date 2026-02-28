using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Discord_clone.Domain.Entities
{
    public class DirectMessage:BaseEntity
    {
        public string Content { get; set; } = string.Empty;
        public DateTime SentAt { get; set; } = DateTime.UtcNow;

        // 1. KİM göndərir?
        public string SenderId { get; set; } = string.Empty;
        public AppUser? Sender { get; set; }

        // 2. KİMƏ göndərir?
        public string ReceiverId { get; set; } = string.Empty;
        public AppUser? Receiver { get; set; }

        // Düzəliş (Edit) funksiyasını bura da qoyuruq
        public bool IsEdited { get; set; } = false;
        public DateTime? UpdatedAt { get; set; }
    }
}
