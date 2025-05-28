using PackingService.Api.DTOs;
using PackingService.Api.Strategies;
using PackingService.Api.Data;
using PackingService.Api.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PackingService.Api.Services
{

    public class PackingService : IPackingService
    {
        private readonly IPackingStrategy _strategy;
        private readonly IEnumerable<BoxDTO> _availableBoxes;
        private readonly PackingDbContext _dbContext;

        public PackingService(
            IPackingStrategy strategy,
            IEnumerable<BoxDTO> availableBoxes,
            PackingDbContext dbContext
        )
        {
            _strategy = strategy;
            _availableBoxes = availableBoxes;
            _dbContext = dbContext;
        }

        public List<OrderPackingResultDTO> PackOrders(IEnumerable<OrderDTO> orders)
        {
            var results = new List<OrderPackingResultDTO>();
            foreach (var order in orders)
            {
                var packed = _strategy.Pack(order.Products, _availableBoxes);
                var apiBoxes = packed
                    .Select(pb => new BoxResultDTO
                    {
                        box_id = pb.BoxType,
                        products = pb.Products,
                        observation = pb.Observacao
                    })
                    .ToList();

                results.Add(new OrderPackingResultDTO
                {
                    order_id = order.OrderId,
                    boxes = apiBoxes
                });
            }
            return results;
        }
    }
}