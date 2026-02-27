using Discord_clone.Application.DTOs;
using Discord_clone.Domain.Entities;
using Discord_clone.Persistence.Contexts;
using Discord_clone.WebApi.Hubs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Discord_clone.WebApi.Controllers
{
    [Authorize] // Ancaq sistemə girənlər mesaj yaza bilər
    [Route("api/[controller]")]
    [ApiController]
    public class MessageController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IHubContext<ChatHub> _hubContext; // SignalR poçtalyonunu bura gətiririk

        public MessageController(AppDbContext context, IHubContext<ChatHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        // 1. READ: Kanala girəndə KÖHNƏ mesajları gətirən API
        [HttpGet("{channelId}")]
        public async Task<IActionResult> GetChannelMessages(Guid channelId)
        {
            var messages = await _context.Messages
                .Include(m => m.Sender) // Kim göndərib onu da çəkirik
                .Where(m => m.ChannelId == channelId)
                .OrderBy(m => m.SentAt) // Köhnədən yeniyə doğru vaxta görə sıralayırıq
                .Select(m => new {
                    m.Id,
                    m.Content,
                    m.SentAt,
                    SenderName = m.Sender!.UserName,
                    // Şəkil yoxdursa, baş hərflərdən ibarət random şəkil veririk
                    AvatarUrl = m.Sender.ProfileImageUrl ?? $"https://ui-avatars.com/api/?name={m.Sender.UserName}&background=random"
                })
                .ToListAsync();

            return Ok(messages);
        }

        // 2. CREATE: Yeni mesaj yaz və SignalR ilə payla
        [HttpPost("send")]
        public async Task<IActionResult> SendMessage([FromBody] SendMessageDto model)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _context.Users.FindAsync(userId);

            if (user == null) return Unauthorized();

            // Görək belə bir kanal həqiqətən varmı?
            var channel = await _context.Channels.FindAsync(model.ChannelId);
            if (channel == null) return NotFound(new { Message = "Kanal tapılmadı!" });

            // 1. Mesajı BAZAYA YAZIRIQ
            var newMessage = new Message
            {
                Content = model.Content,
                ChannelId = model.ChannelId,
                SenderId = userId,
                SentAt = DateTime.UtcNow
            };

            _context.Messages.Add(newMessage);
            await _context.SaveChangesAsync();

            // 2. SignalR İLƏ HƏR KƏSƏ UÇURDURUQ
            var avatar = user.ProfileImageUrl ?? $"https://ui-avatars.com/api/?name={user.UserName}&background=random";

            // "Clients.Group" vasitəsilə sadəcə o kanalda olanlara mesaj gedir
            await _hubContext.Clients.Group(model.ChannelId.ToString())
                .SendAsync("ReceiveMessage", user.UserName, avatar, model.Content);

            return Ok(new { Message = "Mesaj göndərildi!", MessageId = newMessage.Id });
        }
        // 3. UPDATE: Mesajı redaktə et
        [HttpPut("edit/{id}")]
        public async Task<IActionResult> EditMessage(Guid id, [FromBody] UpdateMessageDto model)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var message = await _context.Messages.FindAsync(id);
            if (message == null) return NotFound(new { Message = "Mesaj tapılmadı!" });

            // Təhlükəsizlik: Bu mesajı bu adam yazıb?
            if (message.SenderId != userId)
                return Unauthorized(new { Message = "Sən yalnız öz mesajlarını dəyişdirə bilərsən!" });

            // Mesajı yeniləyirik
            message.Content = model.Content;
            message.IsEdited = true;
            message.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            // SignalR: Otaqdakı hər kəsə xəbər veririk ki, filan ID-li mesaj dəyişdi (ekranda dərhal yenilənsin)
            await _hubContext.Clients.Group(message.ChannelId.ToString())
                .SendAsync("MessageEdited", message.Id, message.Content);

            return Ok(new { Message = "Mesaj uğurla redaktə olundu!" });
        }

        // 4. DELETE: Mesajı sil
        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> DeleteMessage(Guid id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var message = await _context.Messages.FindAsync(id);
            if (message == null) return NotFound(new { Message = "Mesaj tapılmadı!" });

            // Təhlükəsizlik: Bu mesajı bu adam yazıb?
            if (message.SenderId != userId)
                return Unauthorized(new { Message = "Sən yalnız öz mesajlarını silə bilərsən!" });

            var channelId = message.ChannelId; // Silinmədən əvvəl kanalın ID-sini yadda saxlayırıq ki, SignalR-a verək

            _context.Messages.Remove(message);
            await _context.SaveChangesAsync();

            // SignalR: Otaqdakı hər kəsə xəbər veririk ki, filan ID-li mesaj silindi (ekrandan itirsinlər)
            await _hubContext.Clients.Group(channelId.ToString())
                .SendAsync("MessageDeleted", id);

            return Ok(new { Message = "Mesaj uğurla silindi!" });
        }
    }
}
