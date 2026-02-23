using Discord_clone.Application.DTOs;
using Discord_clone.Domain.Entities;
using Discord_clone.Persistence.Contexts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Discord_clone.WebApi.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ServerController : ControllerBase
    {
        private readonly AppDbContext _context;

        // Bazamızı (AppDbContext) Controller-ə çağırırıq
        public ServerController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateServer([FromBody] CreateServerDto model)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId == null)
            {
                return Unauthorized(new { Message = "İstifadəçi tapılmadı!" });
            }

            // Əgər istifadəçi şəkil linki GÖNDƏRMƏYİBSƏ, UI Avatars-dan serverin adına uyğun şəkil yaradırıq.
            string finalImageUrl = string.IsNullOrWhiteSpace(model.ImageUrl)
                ? $"https://ui-avatars.com/api/?name={model.Name}&background=random&color=fff&size=128"
                : model.ImageUrl;

            var newServer = new Server
            {
                Name = model.Name,
                Description = model.Description,
                ImageUrl = finalImageUrl, 
                OwnerId = userId
            };

            _context.Servers.Add(newServer);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Server uğurla yaradıldı!", ServerId = newServer.Id, Image = finalImageUrl });
        }
        // 1. READ: Mənim serverlərimi gətir
        [HttpGet("my-servers")]
        public async Task<IActionResult> GetMyServers()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();

            // Yalnız bu istifadəçinin yaratdığı serverləri tapırıq
            var servers = await _context.Servers
                .Where(s => s.OwnerId == userId)
                .ToListAsync();

            return Ok(servers);
        }

        // 2. UPDATE: Serverin adını və ya şəklini dəyiş
        [HttpPut("update/{id}")]
        public async Task<IActionResult> UpdateServer(Guid id, [FromBody] UpdateServerDto model)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Öncə serveri tapırıq (və yoxlayırıq ki, bu adam o serverin sahibidirmi?)
            var server = await _context.Servers.FirstOrDefaultAsync(s => s.Id == id && s.OwnerId == userId);
            if (server == null) return NotFound(new { Message = "Server tapılmadı və ya buna icazəniz yoxdur!" });

            // Əgər yeni ad göndəribsə, onu dəyiş
            if (!string.IsNullOrWhiteSpace(model.Name))
                server.Name = model.Name;

            // Əgər yeni şəkil göndəribsə, onu dəyiş
            if (!string.IsNullOrWhiteSpace(model.ImageUrl))
                server.ImageUrl = model.ImageUrl;

            await _context.SaveChangesAsync();
            return Ok(new { Message = "Server uğurla yeniləndi!", Server = server });
        }

        // 3. DELETE: Serveri sil
        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> DeleteServer(Guid id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Yenə də ancaq öz serverini silə bilər
            var server = await _context.Servers.FirstOrDefaultAsync(s => s.Id == id && s.OwnerId == userId);
            if (server == null) return NotFound(new { Message = "Server tapılmadı və ya silməyə icazəniz yoxdur!" });

            _context.Servers.Remove(server);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Server uğurla silindi!" });
        }
    }
}
