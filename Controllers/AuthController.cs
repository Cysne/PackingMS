using Microsoft.AspNetCore.Mvc;
using PackingService.Api.DTOs;
using PackingService.Api.Services;

namespace PackingService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
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
    /// Registra um novo usuário no sistema
    /// </summary>
    [HttpPost("register")]
    public async Task<ActionResult<AuthResponseDTO>> Register(RegisterRequestDTO registerRequest)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (await _authService.UserExistsAsync(registerRequest.Email))
                return Conflict(new { message = "Usuário já existe com este email." });

            var result = await _authService.RegisterAsync(registerRequest);
            if (result == null)
                return BadRequest(new { message = "Erro ao registrar usuário." });

            _logger.LogInformation("Usuário {Email} registrado com sucesso", registerRequest.Email);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao registrar usuário {Email}", registerRequest.Email);
            return StatusCode(500, new { message = "Erro interno do servidor." });
        }
    }

    /// <summary>
    /// Autentica um usuário no sistema
    /// </summary>
    [HttpPost("login")]
    public async Task<ActionResult<AuthResponseDTO>> Login(LoginRequestDTO loginRequest)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _authService.LoginAsync(loginRequest);
            if (result == null)
                return Unauthorized(new { message = "Email ou senha inválidos." });

            _logger.LogInformation("Usuário {Email} autenticado com sucesso", loginRequest.Email);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao autenticar usuário {Email}", loginRequest.Email);
            return StatusCode(500, new { message = "Erro interno do servidor." });
        }
    }
}