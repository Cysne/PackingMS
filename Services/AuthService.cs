using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using PackingService.Api.Data;
using PackingService.Api.DTOs;
using PackingService.Api.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace PackingService.Api.Services;

public class AuthService : IAuthService
{
    private readonly PackingDbContext _context;
    private readonly string _jwtKey;
    private readonly string _issuer;
    private readonly string _audience;

    public AuthService(PackingDbContext context)
    {
        _context = context;
        _jwtKey = "MyVeryLongSecretKeyForJWTAuthentication123456789";
        _issuer = "PackingService.Api";
        _audience = "PackingService.Client";
    }

    public async Task<AuthResponseDTO?> LoginAsync(LoginRequestDTO loginRequest)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == loginRequest.Email && u.IsActive);

        if (user == null || !BCrypt.Net.BCrypt.Verify(loginRequest.Password, user.PasswordHash))
        {
            return null;
        }

        var token = GenerateJwtToken(user);
        var expiresAt = DateTime.UtcNow.AddHours(24);

        return new AuthResponseDTO
        {
            Token = token,
            Username = user.Username,
            Email = user.Email,
            ExpiresAt = expiresAt
        };
    }

    public async Task<AuthResponseDTO?> RegisterAsync(RegisterRequestDTO registerRequest)
    {
        if (await UserExistsAsync(registerRequest.Email))
        {
            return null;
        }

        var passwordHash = BCrypt.Net.BCrypt.HashPassword(registerRequest.Password);

        var user = new User
        {
            Username = registerRequest.Username,
            Email = registerRequest.Email,
            PasswordHash = passwordHash,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var token = GenerateJwtToken(user);
        var expiresAt = DateTime.UtcNow.AddHours(24);

        return new AuthResponseDTO
        {
            Token = token,
            Username = user.Username,
            Email = user.Email,
            ExpiresAt = expiresAt
        };
    }

    public async Task<bool> UserExistsAsync(string email)
    {
        return await _context.Users.AnyAsync(u => u.Email == email);
    }

    private string GenerateJwtToken(User user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim("userId", user.Id.ToString())
        };

        var token = new JwtSecurityToken(
            issuer: _issuer,
            audience: _audience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(24),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}