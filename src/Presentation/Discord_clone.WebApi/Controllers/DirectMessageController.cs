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
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class DirectMessageController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IHubContext<ChatHub> _hubContext;

        public DirectMessageController(AppDbContext context, IHubContext<ChatHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        // 1. KÖHNƏ MESAJLARI GƏTİR (Mən və O adam arasındakı)
        [HttpGet("{otherUserId}")]
        public async Task<IActionResult> GetDirectMessages(string otherUserId)
        {
            var myId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var messages = await _context.DirectMessages
                .Include(m => m.Sender)
                .Where(m =>
                    (m.SenderId == myId && m.ReceiverId == otherUserId) ||
                    (m.SenderId == otherUserId && m.ReceiverId == myId))
                .OrderBy(m => m.SentAt)
                .Select(m => new {
                    m.Id,
                    m.Content,
                    m.SentAt,
                    SenderId = m.SenderId,
                    SenderName = m.Sender!.UserName,
                    AvatarUrl = m.Sender.ProfileImageUrl ?? $"https://ui-avatars.com/api/?name={m.Sender.UserName}&background=random"
                })
                .ToListAsync();

            return Ok(messages);
        }

        // 2. YENİ DM GÖNDƏR
        [HttpPost("send")]
        public async Task<IActionResult> SendDirectMessage([FromBody] SendDirectMessageDto model)
        {
            var myId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var me = await _context.Users.FindAsync(myId);

            if (me == null) return Unauthorized();

            // 1. BAZAYA YAZIRIQ
            var newDm = new DirectMessage
            {
                Content = model.Content,
                SenderId = myId,
                ReceiverId = model.ReceiverId,
                SentAt = DateTime.UtcNow
            };

            _context.DirectMessages.Add(newDm);
            await _context.SaveChangesAsync();

            // 2. SIGNALR İLƏ ANINDA GÖNDƏRİRİK
            // Əlifba sırası ilə otaq adını eynilə Hub-dakı kimi tapırıq
            var roomName = string.Compare(myId, model.ReceiverId) < 0
                ? $"{myId}_{model.ReceiverId}"
                : $"{model.ReceiverId}_{myId}";

            var avatar = me.ProfileImageUrl ?? $"https://ui-avatars.com/api/?name={me.UserName}&background=random";

            await _hubContext.Clients.Group(roomName)
                .SendAsync("ReceiveDirectMessage", me.UserName, avatar, model.Content);

            return Ok(new { Message = "DM uğurla göndərildi!", MessageId = newDm.Id });
        }
    }
}