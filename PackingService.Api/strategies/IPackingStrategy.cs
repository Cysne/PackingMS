using PackingService.Api.DTOs;
using System.Collections.Generic;

namespace PackingService.Api.Strategies
{
    /// <summary>
    /// Abstrai uma estrategia de empacotamento de produtos em caixas.
    /// </summary>
    public interface IPackingStrategy
    {
        List<PackedBoxDTO> Pack(
            IEnumerable<ProductDTO> products,
            IEnumerable<BoxDTO> availableBoxes
        );
    }
}
