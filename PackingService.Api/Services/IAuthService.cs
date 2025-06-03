using PackingService.Api.DTOs.Auth;

namespace PackingService.Api.Services
{
    public interface IAuthService
    {
        Task<AuthResponseDTO> LoginAsync(LoginRequestDTO request);
        Task<AuthResponseDTO> RegisterAsync(RegisterRequestDTO request);
        string GenerateJwtToken(int userId, string username, string email);
    }
}
