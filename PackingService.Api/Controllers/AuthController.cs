using Microsoft.AspNetCore.Mvc;
using PackingService.Api.DTOs.Auth;
using PackingService.Api.Services;

namespace PackingService.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        /// <summary>
        /// Realiza login do usuário
        /// </summary>
        /// <param name="request">Dados de login</param>
        /// <returns>Token JWT e informações do usuário</returns>
        [HttpPost("login")]
        public async Task<ActionResult<AuthResponseDTO>> Login([FromBody] LoginRequestDTO request)
        {
            try
            {
                var response = await _authService.LoginAsync(request);
                _logger.LogInformation("Usuário {Username} fez login com sucesso", request.Username);
                return Ok(response);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning("Tentativa de login falhada para usuário {Username}: {Message}", request.Username, ex.Message);
                return Unauthorized(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Registra um novo usuário
        /// </summary>
        /// <param name="request">Dados de registro</param>
        /// <returns>Token JWT e informações do usuário criado</returns>
        [HttpPost("register")]
        public async Task<ActionResult<AuthResponseDTO>> Register([FromBody] RegisterRequestDTO request)
        {
            try
            {
                var response = await _authService.RegisterAsync(request);
                _logger.LogInformation("Usuário {Username} registrado com sucesso", request.Username);
                return Ok(response);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Tentativa de registro falhada para usuário {Username}: {Message}", request.Username, ex.Message);
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
