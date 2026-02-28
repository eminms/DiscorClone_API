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

            // Baxaq görək belə bir server ümumiyyətlə varmı?
            var server = await _context.Servers.FindAsync(model.ServerId);
            if (server == null) return NotFound(new { Message = "Server tapılmadı!" });

            // ROL YOXLAMASI: Bu adam bu serverdə varmı və rolu nədir?
            var memberInfo = await _context.ServerMembers
                .FirstOrDefaultAsync(sm => sm.ServerId == model.ServerId && sm.AppUserId == userId);

            if (memberInfo == null)
                return BadRequest(new { Message = "Sən bu serverin üzvü deyilsən!" });

            if (memberInfo.Role == Discord_clone.Domain.Enums.ServerRole.Member)
                return StatusCode(403, new { Message = "Sənin kanal yaratmaq icazən yoxdur! Yalnız Admin və ya Moderator edə bilər." });

            // Hər şey qaydasındadırsa, kanalı yaradırıq
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

        // 2. READ: Müəyyən bir serverin KANALLARINI gətir (Buna hamı baxa bilər)
        [HttpGet("server/{serverId}")]
        public async Task<IActionResult> GetServerChannels(Guid serverId)
        {
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

            var channel = await _context.Channels.FindAsync(id);
            if (channel == null) return NotFound(new { Message = "Kanal tapılmadı!" });

            // ROL YOXLAMASI
            var memberInfo = await _context.ServerMembers
                .FirstOrDefaultAsync(sm => sm.ServerId == channel.ServerId && sm.AppUserId == userId);

            if (memberInfo == null || memberInfo.Role == Discord_clone.Domain.Enums.ServerRole.Member)
                return StatusCode(403, new { Message = "Bu kanalı dəyişməyə icazəniz yoxdur! Yalnız Admin və ya Moderator edə bilər." });

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

            var channel = await _context.Channels.FindAsync(id);
            if (channel == null) return NotFound(new { Message = "Kanal tapılmadı!" });

            // ROL YOXLAMASI
            var memberInfo = await _context.ServerMembers
                .FirstOrDefaultAsync(sm => sm.ServerId == channel.ServerId && sm.AppUserId == userId);

            if (memberInfo == null || memberInfo.Role == Discord_clone.Domain.Enums.ServerRole.Member)
                return StatusCode(403, new { Message = "Bu kanalı silməyə icazəniz yoxdur! Yalnız Admin və ya Moderator edə bilər." });

            _context.Channels.Remove(channel);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Kanal uğurla silindi!" });
        }
    }
}