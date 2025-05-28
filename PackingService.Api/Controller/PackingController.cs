using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using PackingService.Api.DTOs;
using PackingService.Api.Services;

[ApiController]
[Route("api/[controller]")]
public class PackingController : ControllerBase
{
    private readonly IPackingService _packingService;

    public PackingController(IPackingService packingService)
    {
        _packingService = packingService;
    }

    [HttpPost("pack-orders")]
    public IActionResult PackOrders([FromBody] List<OrderDTO> orders)
    {
        if (orders == null || !orders.Any())
            return BadRequest("No orders provided.");

        var result = _packingService.PackOrders(orders);
        return Ok(result);
    }
}