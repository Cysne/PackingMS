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
                try
                {
                    var packed = _strategy.Pack(order.Products, _availableBoxes);

                    var orderEntity = new OrderEntity
                    {
                        OrderDate = DateTime.UtcNow,
                        OrderItems = new List<OrderItemEntity>(),
                        OrderBoxes = new List<OrderBoxEntity>()
                    };

                    foreach (var product in order.Products)
                    {
                        var existingProduct = _dbContext.Products
                            .FirstOrDefault(p => p.Name == product.Name &&
                                               p.Height == product.Height &&
                                               p.Width == product.Width &&
                                               p.Length == product.Length);

                        if (existingProduct == null)
                        {
                            existingProduct = new ProductEntity
                            {
                                Name = product.Name,
                                Height = product.Height,
                                Width = product.Width,
                                Length = product.Length
                            };
                            _dbContext.Products.Add(existingProduct);
                            _dbContext.SaveChanges();
                        }

                        orderEntity.OrderItems.Add(new OrderItemEntity
                        {
                            ProductId = existingProduct.ProductId,
                            Quantity = 1
                        });
                    }

                    _dbContext.Orders.Add(orderEntity);
                    _dbContext.SaveChanges();

                    foreach (var packedBox in packed)
                    {
                        var boxEntity = _dbContext.Boxes
                            .FirstOrDefault(b => b.BoxType == packedBox.BoxType);

                        if (boxEntity != null)
                        {
                            var orderBox = new OrderBoxEntity
                            {
                                OrderId = orderEntity.OrderId,
                                BoxId = boxEntity.BoxId,
                                Observation = packedBox.Observacao
                            };
                            _dbContext.OrderBoxes.Add(orderBox);
                        }
                    }

                    _dbContext.SaveChanges();

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
                        order_id = orderEntity.OrderId,
                        boxes = apiBoxes
                    });
                }
                catch (Exception)
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
                        order_id = 0,
                        boxes = apiBoxes
                    });
                }
            }

            return results;
        }
    }
}