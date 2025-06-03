using Microsoft.IdentityModel.Tokens;
using PackingService.Api.Data;
using PackingService.Api.DTOs.Auth;
using PackingService.Api.Entities;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BCrypt.Net;
using Microsoft.EntityFrameworkCore;

namespace PackingService.Api.Services
{
    public class AuthService : IAuthService
    {
        private readonly PackingDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthService(PackingDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public async Task<AuthResponseDTO> LoginAsync(LoginRequestDTO request)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == request.Username && u.IsActive);

            if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            {
                throw new UnauthorizedAccessException("Credenciais inválidas");
            }

            var token = GenerateJwtToken(user.UserId, user.Username, user.Email);
            var expiryHours = int.Parse(_configuration["JwtSettings:ExpiryHours"] ?? _configuration["Jwt:ExpiryHours"] ?? "24");

            return new AuthResponseDTO
            {
                Token = token,
                Username = user.Username,
                Email = user.Email,
                ExpiresAt = DateTime.UtcNow.AddHours(expiryHours)
            };
        }

        public async Task<AuthResponseDTO> RegisterAsync(RegisterRequestDTO request)
        {
            var existingUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == request.Username || u.Email == request.Email);

            if (existingUser != null)
            {
                throw new InvalidOperationException("Usuário ou email já existe");
            }

            var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

            var user = new User
            {
                Username = request.Username,
                Email = request.Email,
                PasswordHash = passwordHash,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var token = GenerateJwtToken(user.UserId, user.Username, user.Email);
            var expiryHours = int.Parse(_configuration["JwtSettings:ExpiryHours"] ?? _configuration["Jwt:ExpiryHours"] ?? "24");

            return new AuthResponseDTO
            {
                Token = token,
                Username = user.Username,
                Email = user.Email,
                ExpiresAt = DateTime.UtcNow.AddHours(expiryHours)
            };
        }

        public string GenerateJwtToken(int userId, string username, string email)
        {
            // Configuração JWT padronizada usando IConfiguration
            var jwtKey = _configuration["JwtSettings:SecretKey"] ?? _configuration["Jwt:Key"] ?? "SuperSecretKeyWithAtLeast32Characters123!";
            var jwtIssuer = _configuration["JwtSettings:Issuer"] ?? _configuration["Jwt:Issuer"] ?? "PackingService.Api";
            var jwtAudience = _configuration["JwtSettings:Audience"] ?? _configuration["Jwt:Audience"] ?? "PackingService.Api";
            var expiryHours = int.Parse(_configuration["JwtSettings:ExpiryHours"] ?? _configuration["Jwt:ExpiryHours"] ?? "24");

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim("nameid", userId.ToString()),
                new Claim(ClaimTypes.Name, username),
                new Claim(ClaimTypes.Email, email),
                new Claim(JwtRegisteredClaimNames.Sub, username),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
            };

            var token = new JwtSecurityToken(
                issuer: jwtIssuer,
                audience: jwtAudience,
                claims: claims,
                expires: DateTime.UtcNow.AddHours(expiryHours),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
