using System.Collections.Generic;

namespace PackingService.Api.DTOs
{
    public class OrderPackingResultDTO
    {
        public int order_id { get; set; }
        public List<BoxResultDTO> boxes { get; set; } = new List<BoxResultDTO>();
    }
}
