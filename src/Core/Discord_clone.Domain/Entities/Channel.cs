using Discord_clone.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Discord_clone.Domain.Entities
{
    public class Channel:BaseEntity
    {
        public string Name { get; set; } = string.Empty;

        public ChannelType Type { get; set; } = ChannelType.Text;

        public Guid ServerId { get; set; }

        public Server? Server { get; set; }
    }
}
