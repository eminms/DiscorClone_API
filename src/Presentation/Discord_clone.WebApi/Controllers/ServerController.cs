using Discord_clone.Application.DTOs;
using Discord_clone.Domain.Entities;
using Discord_clone.Persistence.Contexts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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
    }
}
