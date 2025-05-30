using System.Collections.Generic;
using System.Linq;
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
        if (orders == null || !orders.Any())
            return BadRequest("No orders provided.");

        foreach (var order in orders)
        {
            if (order.Products == null || !order.Products.Any())
                return BadRequest($"O pedido {order.OrderId} não possui produtos.");

            foreach (var product in order.Products)
            {
                if (string.IsNullOrWhiteSpace(product.Name))
                    return BadRequest($"Produto do pedido {order.OrderId} está com nome vazio ou nulo.");
                if (product.Height <= 0 || product.Width <= 0 || product.Length <= 0)
                    return BadRequest($"Produto '{product.Name}' do pedido {order.OrderId} possui dimensões inválidas (todas devem ser maiores que zero).");
            }
        }

        try
        {
            var result = _packingService.PackOrders(orders);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao processar o pedido no endpoint pack-orders.");
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }
}