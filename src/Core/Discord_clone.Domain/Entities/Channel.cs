using Discord_clone.Domain.Enums;
namespace Discord_clone.Domain.Entities
{
    public class Channel:BaseEntity
    {
        public string Name { get; set; } = string.Empty;

        public ChannelType Type { get; set; } = ChannelType.Text;

        public Guid ServerId { get; set; }

        public Server? Server { get; set; }
        public ICollection<Message> Messages { get; set; } = new List<Message>();
    }
}
