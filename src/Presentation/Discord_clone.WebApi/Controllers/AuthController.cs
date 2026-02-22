using Discord_clone.Application.DTOs;
using Discord_clone.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Discord_clone.WebApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly UserManager<AppUser> _userManager;
    private readonly IConfiguration _configuration;

    // IConfiguration əlavə etdik ki, appsettings.json-dan Key-i oxuya bilək
    public AuthController(UserManager<AppUser> userManager, IConfiguration configuration)
    {
        _userManager = userManager;
        _configuration = configuration;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto model)
    {
        string defaultAvatar = $"https://ui-avatars.com/api/?name={model.Username}&background=random&color=fff&size=128";
        var user = new AppUser
        {
            UserName = model.Username,
            Email = model.Email,
            ProfileImageUrl = defaultAvatar
        };

        var result = await _userManager.CreateAsync(user, model.Password);

        if (result.Succeeded)
        {
            return Ok(new { Message = "İstifadəçi uğurla yaradıldı!" });
        }

        return BadRequest(result.Errors);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto model)
    {
        // 1. İstifadəçini bazada axtarırıq
        var user = await _userManager.FindByNameAsync(model.Username);

        // 2. Əgər istifadəçi tapılmasa və ya şifrə səhvdirsə
        if (user == null || !await _userManager.CheckPasswordAsync(user, model.Password))
        {
            return Unauthorized(new { Message = "İstifadəçi adı və ya şifrə yanlışdır!" });
        }

        // 3. Əgər məlumatlar düzdürsə, Token  yaratmağa başlayırıq
        var authClaims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, user.UserName!),
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        // appsettings.json-dan ayarları oxuyuruq
        var jwtSettings = _configuration.GetSection("Jwt");
        var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]!));

        // Tokeni yığırıq (möhürləyirik)
        var token = new JwtSecurityToken(
            issuer: jwtSettings["Issuer"],
            audience: jwtSettings["Audience"],
            expires: DateTime.Now.AddDays(Convert.ToDouble(jwtSettings["ExpireDays"])),
            claims: authClaims,
            signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
        );

        // 4. Tokeni istifadəçiyə qaytarırıq
        return Ok(new
        {
            Token = new JwtSecurityTokenHandler().WriteToken(token),
            Expiration = token.ValidTo
        });
    }
}