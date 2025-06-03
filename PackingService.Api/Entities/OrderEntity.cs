using System;
using System.Collections.Generic;
namespace PackingService.Api.Entities
{
    public class OrderEntity
    {
        public int OrderId { get; set; }
        public DateTime OrderDate { get; set; }
        public int UserId { get; set; }
        public User? User { get; set; }
        public ICollection<OrderItemEntity>? OrderItems { get; set; }
        public ICollection<OrderBoxEntity>? OrderBoxes { get; set; }
    }
}