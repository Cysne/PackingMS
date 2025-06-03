using PackingService.Api.DTOs;
using System.Collections.Generic;

namespace PackingService.Api.Services
{
    public interface IPackingService
    {
        List<OrderPackingResultDTO> PackOrders(IEnumerable<OrderDTO> orders, int userId);
    }
}