using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using PackingService.Api.DTOs;
using PackingService.Api.Services;
using Microsoft.Extensions.Logging;

[Authorize]
[ApiController]
[Route("api/[controller]")]

public class PackingController : ControllerBase
{
    private readonly IPackingService _packingService;
    private readonly ILogger<PackingController> _logger;

    public PackingController(IPackingService packingService, ILogger<PackingController> logger)
    {
        _packingService = packingService;
        _logger = logger;
    }

    [HttpPost("pack-orders")]
    public IActionResult PackOrders([FromBody] List<OrderDTO> orders)
    {
        _logger.LogInformation("Iniciando processamento de {OrderCount} pedidos", orders?.Count ?? 0);

        if (orders == null || !orders.Any())
        {
            _logger.LogWarning(" Requisição rejeitada: nenhum pedido fornecido");
            return BadRequest("No orders provided.");
        }

        foreach (var order in orders)
        {
            if (order.Products == null || !order.Products.Any())
            {
                _logger.LogWarning(" Pedido {OrderId} rejeitado: sem produtos", order.OrderId);
                return BadRequest($"O pedido {order.OrderId} não possui produtos.");
            }

            foreach (var product in order.Products)
            {
                if (string.IsNullOrWhiteSpace(product.Name))
                {
                    _logger.LogWarning(" Produto do pedido {OrderId} rejeitado: nome vazio", order.OrderId);
                    return BadRequest($"Produto do pedido {order.OrderId} está com nome vazio ou nulo.");
                }
                if (product.Height <= 0 || product.Width <= 0 || product.Length <= 0)
                {
                    _logger.LogWarning(" Produto '{ProductName}' do pedido {OrderId} rejeitado: dimensões inválidas ({Height}x{Width}x{Length})",
                        product.Name, order.OrderId, product.Height, product.Width, product.Length);
                    return BadRequest($"Produto '{product.Name}' do pedido {order.OrderId} possui dimensões inválidas (todas devem ser maiores que zero).");
                }
            }
            _logger.LogInformation(" Pedido {OrderId} validado com sucesso - {ProductCount} produtos", order.OrderId, order.Products.Count);
        }
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                _logger.LogWarning("Usuário não autenticado ou ID inválido");
                return Unauthorized("Usuário não autenticado ou ID inválido.");
            }
            _logger.LogInformation("Iniciando empacotamento dos pedidos para o usuário {UserId}...", userId);
            var result = _packingService.PackOrders(orders, userId);
            _logger.LogInformation("Empacotamento concluído com sucesso - {ResultCount} resultados gerados", result.Count);

            var hasFailures = result.Any(r => r.order_id == 0);

            if (hasFailures)
            {
                var successCount = result.Count(r => r.order_id > 0);
                var failureCount = result.Count(r => r.order_id == 0);
                _logger.LogWarning("Processamento parcial: {SuccessCount} sucessos, {FailureCount} falhas", successCount, failureCount);
                return StatusCode(207, result);
            }

            _logger.LogInformation("Todos os pedidos processados com sucesso");
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, " Erro ao processar o pedido no endpoint pack-orders");
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }
}