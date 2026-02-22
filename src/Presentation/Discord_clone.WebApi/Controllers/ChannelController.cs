using Discord_clone.Application.DTOs;
using Discord_clone.Domain.Entities;
using Discord_clone.Persistence.Contexts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace Discord_clone.WebApi.Controllers
{
    [Authorize] // Bura da ancaq Tokenlə girmək olar!
    [Route("api/[controller]")]
    [ApiController]
    public class ChannelController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ChannelController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateChannel([FromBody] CreateChannelDto model)
        {
            // 1. Yeni Kanal obyektini yığırıq
            var newChannel = new Channel
            {
                Name = model.Name,
                Type = model.Type,
                ServerId = model.ServerId // Hansı serverin içinə gedəcək?
            };

            // 2. Bazaya əlavə edirik
            _context.Channels.Add(newChannel);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Kanal uğurla yaradıldı!", ChannelId = newChannel.Id });
        }
    }
}