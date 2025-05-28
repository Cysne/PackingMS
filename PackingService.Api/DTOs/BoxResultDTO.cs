using System.Collections.Generic;

namespace PackingService.Api.DTOs
{
    public class BoxResultDTO
    {
        public string? box_id { get; set; }
        public List<string> products { get; set; } = new List<string>();
        public string? observation { get; set; }
    }
}
