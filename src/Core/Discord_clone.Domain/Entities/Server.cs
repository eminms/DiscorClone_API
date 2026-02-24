using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Discord_clone.Domain.Entities
{
    public class Server:BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string ImageUrl { get; set; }
        public string OwnerId { get; set; } = string.Empty; // Bu serveri kim yaradıb? (AppUser Id)
        public AppUser? Owner { get; set; }
        public ICollection<Channel> Channels { get; set; } = new List<Channel>();
        public ICollection<ServerMember> Members { get; set; } = new List<ServerMember>();
    }
}
