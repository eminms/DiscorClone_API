using Discord_clone.Application.DTOs;
using Discord_clone.Domain.Entities;
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
    public class ChannelController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ChannelController(AppDbContext context)
        {
            _context = context;
        }

        // 1. CREATE: Kanal yarat
        [HttpPost("create")]
        public async Task<IActionResult> CreateChannel([FromBody] CreateChannelDto model)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Təhlükəsizlik: Görək bu server həqiqətən bu istifadəçiyə aiddir?
            var server = await _context.Servers.FirstOrDefaultAsync(s => s.Id == model.ServerId);
            if (server == null) return NotFound(new { Message = "Server tapılmadı!" });
            if (server.OwnerId != userId) return Unauthorized(new { Message = "Bu serverdə kanal yaratmaq üçün icazəniz yoxdur!" });

            var newChannel = new Channel
            {
                Name = model.Name,
                Type = model.Type,
                ServerId = model.ServerId
            };

            _context.Channels.Add(newChannel);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Kanal uğurla yaradıldı!", ChannelId = newChannel.Id });
        }

        // 2. READ: Müəyyən bir serverin KANALLARINI gətir
        [HttpGet("server/{serverId}")]
        public async Task<IActionResult> GetServerChannels(Guid serverId)
        {
            // Sadəcə bu ServerId-yə aid olan kanalları listələyirik
            var channels = await _context.Channels
                .Where(c => c.ServerId == serverId)
                .ToListAsync();

            return Ok(channels);
        }

        // 3. UPDATE: Kanalın adını dəyiş
        [HttpPut("update/{id}")]
        public async Task<IActionResult> UpdateChannel(Guid id, [FromBody] UpdateChannelDto model)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Kanalı tapırıq və ".Include(c => c.Server)" ilə onun aid olduğu serverin məlumatlarını da gətiririk ki, sahibini yoxlayaq
            var channel = await _context.Channels
                .Include(c => c.Server)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (channel == null) return NotFound(new { Message = "Kanal tapılmadı!" });
            if (channel.Server.OwnerId != userId) return Unauthorized(new { Message = "Bu kanalı dəyişməyə icazəniz yoxdur!" });

            if (!string.IsNullOrWhiteSpace(model.Name))
                channel.Name = model.Name;

            await _context.SaveChangesAsync();
            return Ok(new { Message = "Kanal uğurla yeniləndi!", Channel = channel });
        }

        // 4. DELETE: Kanalı sil
        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> DeleteChannel(Guid id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var channel = await _context.Channels
                .Include(c => c.Server)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (channel == null) return NotFound(new { Message = "Kanal tapılmadı!" });
            if (channel.Server.OwnerId != userId) return Unauthorized(new { Message = "Bu kanalı silməyə icazəniz yoxdur!" });

            _context.Channels.Remove(channel);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Kanal uğurla silindi!" });
        }
    }
}