using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;

using PackingService.Api.DTOs;
using PackingService.Api.Services;
using Microsoft.Extensions.Logging;

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