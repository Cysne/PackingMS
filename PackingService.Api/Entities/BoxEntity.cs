using System.Collections.Generic;
namespace PackingService.Api.Entities
{
    public class BoxEntity
    {
        public int BoxId { get; set; }
        public string? BoxType { get; set; }
        public decimal Height { get; set; }
        public decimal Width { get; set; }
        public decimal Length { get; set; }
        public ICollection<OrderBoxEntity>? OrderBoxes { get; set; }
    }
}