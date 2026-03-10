using Discord_clone.Domain.Entities;
using Discord_clone.Domain.Enums;
using Discord_clone.Persistence.Contexts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Linq; // LINQ xətası (Select/Where) verməməsi üçün bu mütləqdir!

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

        // 1. USERNAME İLƏ DOSTLUQ İSTƏYİ GÖNDƏR (HTML-dəki input üçün)
        [HttpPost("request-by-username/{username}")]
        public async Task<IActionResult> SendRequestByUsername(string username)
        {
            var senderId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Yazılan ada uyğun adamı bazadan tapırıq
            var receiver = await _context.Users.FirstOrDefaultAsync(u => u.UserName == username);

            if (receiver == null)
                return NotFound(new { Message = "Belə bir istifadəçi tapılmadı!" });

            if (senderId == receiver.Id)
                return BadRequest(new { Message = "Özünə dostluq ata bilməzsən! 😅" });

            // Əlaqəni yoxlayırıq (Həm mənim ona, həm onun mənə atdığı)
            var existingRelation = await _context.Friendships
                .FirstOrDefaultAsync(f =>
                    (f.RequesterId == senderId && f.ReceiverId == receiver.Id) ||
                    (f.RequesterId == receiver.Id && f.ReceiverId == senderId));

            if (existingRelation != null)
            {
                if (existingRelation.Status == FriendshipStatus.Accepted)
                    return BadRequest(new { Message = "Siz onsuz da dostsunuz!" });

                return BadRequest(new { Message = "Artıq istək göndərilib və ya gözləmədədir!" });
            }

            // Hər şey qaydasındadırsa, yeni istək yaradırıq
            var friendship = new Friendship
            {
                RequesterId = senderId!,
                ReceiverId = receiver.Id,
                Status = FriendshipStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };

            _context.Friendships.Add(friendship);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Dostluq istəyi uğurla göndərildi! ✅" });
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

        // 3. DOSTLUĞU SİL VƏ YA İSTƏYİ RƏDD ET
        [HttpDelete("remove/{otherUserId}")]
        public async Task<IActionResult> RemoveFriendOrDecline(string otherUserId)
        {
            var myId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Aradakı hər hansı bir əlaqəni tapırıq
            var relation = await _context.Friendships
                .FirstOrDefaultAsync(f =>
                    (f.RequesterId == myId && f.ReceiverId == otherUserId) ||
                    (f.RequesterId == otherUserId && f.ReceiverId == myId));

            if (relation == null)
                return NotFound(new { Message = "Silinəcək heç bir əlaqə tapılmadı." });

            // Bazadan uçurduruq
            _context.Friendships.Remove(relation);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Əlaqə uğurla silindi/rədd edildi." });
        }

        // 4. DOSTLARIMIN SİYAHISINI GƏTİR (HTML-dəki "Hamısı" tabı üçün)
        [HttpGet("friends")]
        public async Task<IActionResult> GetMyFriends()
        {
            var myId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Statusu "Accepted" olan əlaqələri tapırıq
            var friendships = await _context.Friendships
                .Include(f => f.Requester)
                .Include(f => f.Receiver)
                .Where(f => (f.RequesterId == myId || f.ReceiverId == myId) && f.Status == FriendshipStatus.Accepted)
                .ToListAsync();

            // Mənə dostlarımın məlumatları lazımdır (Öz məlumatım yox)
            var friends = friendships.Select(f =>
            {
                var friend = f.RequesterId == myId ? f.Receiver : f.Requester;
                return new
                {
                    Id = friend.Id,
                    UserName = friend.UserName
                };
            }).ToList();

            return Ok(friends);
        }

        // 5. MƏNƏ GƏLƏN VƏ GÖZLƏYƏN İSTƏKLƏR (HTML-dəki "Gözləyənlər" tabı üçün)
        [HttpGet("pending")]
        public async Task<IActionResult> GetPendingRequests()
        {
            var myId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Mənə gələn və hələ təsdiqlənməyən (Pending) istəklər
            var pendingRequests = await _context.Friendships
                .Include(f => f.Requester)
                .Where(f => f.ReceiverId == myId && f.Status == FriendshipStatus.Pending)
                .Select(f => new
                {
                    Id = f.Requester.Id,
                    UserName = f.Requester.UserName
                })
                .ToListAsync();

            return Ok(pendingRequests);
        }
    }
}