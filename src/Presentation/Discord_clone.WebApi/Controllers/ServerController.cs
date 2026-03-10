using Discord_clone.Application.DTOs;
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
    public class ServerController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ServerController(AppDbContext context)
        {
            _context = context;
        }

        // 1. CREATE: Server Yarat və Avtomatik Dəvət Kodu ver
        [HttpPost("create")]
        public async Task<IActionResult> CreateServer([FromBody] CreateServerDto model)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId == null)
                return Unauthorized(new { Message = "İstifadəçi tapılmadı!" });

            string finalImageUrl = string.IsNullOrWhiteSpace(model.ImageUrl)
                ? $"https://ui-avatars.com/api/?name={model.Name}&background=random&color=fff&size=128"
                : model.ImageUrl;

            // YENİ ƏLAVƏ: 6 simvolluq unikal dəvət kodu yaradırıq (Məsələn: "A8F2B9")
            string generatedInviteCode = Guid.NewGuid().ToString().Substring(0, 6).ToUpper();

            // Öncə Serveri yaradırıq
            var newServer = new Server
            {
                Name = model.Name,
                Description = model.Description,
                ImageUrl = finalImageUrl,
                OwnerId = userId,
                InviteCode = generatedInviteCode // Kodu bazaya yazırıq
            };

            _context.Servers.Add(newServer);
            await _context.SaveChangesAsync(); // newServer.Id yaransın deyə yadda saxlayırıq

            // Serveri yaradan adamı "Admin" olaraq əlavə edirik
            var member = new ServerMember
            {
                AppUserId = userId,
                ServerId = newServer.Id,
                Role = ServerRole.Admin,
                JoinedAt = DateTime.UtcNow
            };

            _context.ServerMembers.Add(member);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                Message = "Server uğurla yaradıldı və siz Admin təyin olundunuz!",
                ServerId = newServer.Id,
                Image = finalImageUrl,
                InviteCode = generatedInviteCode // Ekranda göstərmək üçün kodu da qaytarırıq
            });
        }

        // 2. READ: Mənim yaratdığım serverləri gətir
        [HttpGet("my-servers")]
        public async Task<IActionResult> GetMyServers()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();

            var servers = await _context.Servers
                .Where(s => s.OwnerId == userId)
                .ToListAsync();

            return Ok(servers);
        }

        // 3. UPDATE: Serverin adını və ya şəklini dəyiş
        [HttpPut("update/{id}")]
        public async Task<IActionResult> UpdateServer(Guid id, [FromBody] UpdateServerDto model)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var server = await _context.Servers.FirstOrDefaultAsync(s => s.Id == id && s.OwnerId == userId);
            if (server == null) return NotFound(new { Message = "Server tapılmadı və ya buna icazəniz yoxdur!" });

            if (!string.IsNullOrWhiteSpace(model.Name))
                server.Name = model.Name;

            if (!string.IsNullOrWhiteSpace(model.ImageUrl))
                server.ImageUrl = model.ImageUrl;

            await _context.SaveChangesAsync();
            return Ok(new { Message = "Server uğurla yeniləndi!", Server = server });
        }

        // 4. DELETE: Serveri sil
        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> DeleteServer(Guid id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var server = await _context.Servers.FirstOrDefaultAsync(s => s.Id == id && s.OwnerId == userId);
            if (server == null) return NotFound(new { Message = "Server tapılmadı və ya silməyə icazəniz yoxdur!" });

            _context.Servers.Remove(server);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Server uğurla silindi!" });
        }

        // 5. JOIN: Dəvət Kodu ilə başqasının serverinə qoşulmaq
        [HttpPost("join/{inviteCode}")]
        public async Task<IActionResult> JoinServer(string inviteCode)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();

            var server = await _context.Servers.FirstOrDefaultAsync(s => s.InviteCode == inviteCode);
            if (server == null) return NotFound(new { Message = "Yanlış və ya vaxtı keçmiş dəvət kodu!" });

            var existingMember = await _context.ServerMembers
                .FirstOrDefaultAsync(sm => sm.ServerId == server.Id && sm.AppUserId == userId);

            if (existingMember != null)
                return BadRequest(new { Message = "Sən onsuz da bu serverin üzvüsən!" });

            var newMember = new ServerMember
            {
                AppUserId = userId,
                ServerId = server.Id,
                Role = ServerRole.Member, // Qoşulan adam Adi Üzv olur
                JoinedAt = DateTime.UtcNow
            };

            _context.ServerMembers.Add(newMember);
            await _context.SaveChangesAsync();

            return Ok(new { Message = $"Təbriklər! '{server.Name}' serverinə uğurla qoşuldun. 🎉" });
        }

        // 6. READ: Mənim ÜZV OLDUĞUM (qoşulduğum) bütün serverləri gətir
        [HttpGet("joined-servers")]
        public async Task<IActionResult> GetJoinedServers()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();

            var joinedServers = await _context.ServerMembers
                .Where(sm => sm.AppUserId == userId)
                .Include(sm => sm.Server)
                .Select(sm => sm.Server)
                .ToListAsync();

            return Ok(joinedServers);
        }

        // 7. ROLES: Serverdə kiməsə rol vermək (Yalnız Admin edə bilər)
        [HttpPut("{serverId}/roles")]
        public async Task<IActionResult> UpdateMemberRole(Guid serverId, [FromBody] UpdateRoleDto model)
        {
            var myId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var myMemberInfo = await _context.ServerMembers
                .FirstOrDefaultAsync(sm => sm.ServerId == serverId && sm.AppUserId == myId);

            if (myMemberInfo == null)
                return NotFound(new { Message = "Sən bu serverdə yoxsan!" });

            if (myMemberInfo.Role != ServerRole.Admin)
                return StatusCode(403, new { Message = "Sənin başqalarına rol vermək icazən yoxdur! Yalnız Admin edə bilər." });

            if (myId == model.TargetUserId)
                return BadRequest(new { Message = "Sən öz rolunu dəyişə bilməzsən (Serverin tək Admini sən olmalısan)!" });

            var targetMember = await _context.ServerMembers
                .FirstOrDefaultAsync(sm => sm.ServerId == serverId && sm.AppUserId == model.TargetUserId);

            if (targetMember == null)
                return NotFound(new { Message = "Rol vermək istədiyin adam bu serverin üzvü deyil!" });

            targetMember.Role = model.NewRole;
            await _context.SaveChangesAsync();

            return Ok(new { Message = "İstifadəçinin rolu uğurla yeniləndi!" });
        }
    }
}