using Discord_clone.Domain.Entities;
using Discord_clone.Domain.Enums;
using Discord_clone.Persistence.Contexts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Discord_clone.WebApi.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class FriendshipController : ControllerBase
    {
        private readonly AppDbContext _context;

        public FriendshipController(AppDbContext context)
        {
            _context = context;
        }

        // 1. DOSTLUQ İSTƏYİ GÖNDƏR
        [HttpPost("request/{receiverId}")]
        public async Task<IActionResult> SendFriendRequest(string receiverId)
        {
            var senderId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (senderId == receiverId)
                return BadRequest(new { Message = "Özünə dostluq ata bilməzsən! 😅" });

            // Görək belə bir adam varmı?
            var receiverExists = await _context.Users.AnyAsync(u => u.Id == receiverId);
            if (!receiverExists) return NotFound(new { Message = "İstifadəçi tapılmadı!" });

            // Görək onsuz da aralarında bir əlaqə (Dostluq və ya İstək) varmı?
            // Həm Mənim ona, həm də Onun mənə atdığı istəkləri yoxlayırıq
            var existingRelation = await _context.Friendships
                .FirstOrDefaultAsync(f =>
                    (f.RequesterId == senderId && f.ReceiverId == receiverId) ||
                    (f.RequesterId == receiverId && f.ReceiverId == senderId));

            if (existingRelation != null)
            {
                if (existingRelation.Status == FriendshipStatus.Accepted)
                    return BadRequest(new { Message = "Siz onsuz da dostsunuz!" });

                return BadRequest(new { Message = "Artıq istək göndərilib və ya gözləmədədir!" });
            }

            // Hər şey təmizdirsə, yeni istək yaradırıq (Pending statusu ilə)
            var friendship = new Friendship
            {
                RequesterId = senderId!,
                ReceiverId = receiverId,
                Status = FriendshipStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };

            _context.Friendships.Add(friendship);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Dostluq istəyi uğurla göndərildi!" });
        }

        // 2. DOSTLUQ İSTƏYİNİ QƏBUL ET
        [HttpPut("accept/{requesterId}")]
        public async Task<IActionResult> AcceptFriendRequest(string requesterId)
        {
            var myId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Mənə gələn o istəyi bazadan tapırıq
            var request = await _context.Friendships
                .FirstOrDefaultAsync(f => f.RequesterId == requesterId && f.ReceiverId == myId && f.Status == FriendshipStatus.Pending);

            if (request == null)
                return NotFound(new { Message = "Belə bir dostluq istəyi tapılmadı!" });

            // Statusunu 'Accepted' edirik
            request.Status = FriendshipStatus.Accepted;
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Dostluq istəyi qəbul edildi! 🎉" });
        }

        // 3. DOSTLUĞU SİL VƏ YA İSTƏYİ RƏDD ET (Variant 1 məntiqi)
        [HttpDelete("remove/{otherUserId}")]
        public async Task<IActionResult> RemoveFriendOrDecline(string otherUserId)
        {
            var myId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Aradakı hər hansı bir əlaqəni (İstər mən göndərim, istər o) tapırıq
            var relation = await _context.Friendships
                .FirstOrDefaultAsync(f =>
                    (f.RequesterId == myId && f.ReceiverId == otherUserId) ||
                    (f.RequesterId == otherUserId && f.ReceiverId == myId));

            if (relation == null)
                return NotFound(new { Message = "Silinəcək heç bir əlaqə tapılmadı." });

            // Sadəcə bazadan uçurduruq
            _context.Friendships.Remove(relation);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Əlaqə uğurla silindi/rədd edildi." });
        }
    }
}
