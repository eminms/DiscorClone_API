using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Discord_clone.Domain.Entities
{
    public class ServerMember
    {
        // Kim qoşulub?
        public string AppUserId { get; set; } = string.Empty;
        public AppUser? AppUser { get; set; }

        // Hansı serverə qoşulub?
        public Guid ServerId { get; set; }
        public Server? Server { get; set; }

        // Nə vaxt qoşulub? 
        public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
    }
}
