using PackingService.Api.DTOs;

namespace PackingService.Api.Services;

public interface IAuthService
{
    Task<AuthResponseDTO?> LoginAsync(LoginRequestDTO loginRequest);
    Task<AuthResponseDTO?> RegisterAsync(RegisterRequestDTO registerRequest);
    Task<bool> UserExistsAsync(string email);
}