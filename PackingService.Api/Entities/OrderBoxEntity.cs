using System.Collections.Generic;
namespace PackingService.Api.Entities
{
    public class OrderBoxEntity
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public int BoxId { get; set; }
        public string? Observation { get; set; }

        public OrderEntity? Order { get; set; }
        public BoxEntity? Box { get; set; }
    }
}
