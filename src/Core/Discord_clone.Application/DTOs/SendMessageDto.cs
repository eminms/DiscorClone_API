namespace Discord_clone.Application.DTOs
{
    public class SendMessageDto
    {
        // Hansı kanala göndəririk?
        public Guid ChannelId { get; set; }

        // Nə yazırıq?
        public string Content { get; set; } = string.Empty;
    }
}
