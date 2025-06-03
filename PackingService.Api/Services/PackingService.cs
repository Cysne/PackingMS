using PackingService.Api.DTOs;
using PackingService.Api.Strategies;
using PackingService.Api.Data;
using PackingService.Api.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PackingService.Api.Services
{
    public class PackingService : IPackingService
    {
        private readonly IPackingStrategy _strategy;
        private readonly PackingDbContext _dbContext;
        private readonly ILogger<PackingService> _logger;

        public PackingService(
            IPackingStrategy strategy,
            PackingDbContext dbContext,
            ILogger<PackingService> logger
        )
        {
            _strategy = strategy;
            _dbContext = dbContext;
            _logger = logger;
        }

        public List<OrderPackingResultDTO> PackOrders(IEnumerable<OrderDTO> orders, int userId)
        {
            var results = new List<OrderPackingResultDTO>();

            var availableBoxes = _dbContext.Boxes
                .Select(b => new BoxDTO
                {
                    BoxType = b.BoxType ?? string.Empty,
                    Height = b.Height,
                    Width = b.Width,
                    Length = b.Length
                })
                .ToList();

            foreach (var order in orders)
            {
                var packedResult = _strategy.Pack(order.Products, availableBoxes); var apiBoxes = packedResult
                    .Select(pb => new BoxResultDTO
                    {
                        box_id = pb.BoxType,
                        products = pb.Products,
                        observation = pb.Observacao
                    })
                    .ToList();

                var isInMemoryDatabase = _dbContext.Database.ProviderName == "Microsoft.EntityFrameworkCore.InMemory";

                if (isInMemoryDatabase)
                {
                    try
                    {
                        var orderEntity = ExecuteOrderPersistence(order, packedResult, userId);

                        results.Add(new OrderPackingResultDTO
                        {
                            order_id = orderEntity.OrderId,
                            boxes = apiBoxes
                        });
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Erro ao persistir o pedido {OriginalOrderId}. Operação revertida.", order.OrderId);

                        results.Add(new OrderPackingResultDTO
                        {
                            order_id = 0,
                            boxes = apiBoxes
                        });
                    }
                }
                else
                {
                    using (var transaction = _dbContext.Database.BeginTransaction())
                    {
                        try
                        {
                            var orderEntity = ExecuteOrderPersistence(order, packedResult, userId);

                            transaction.Commit();

                            results.Add(new OrderPackingResultDTO
                            {
                                order_id = orderEntity.OrderId,
                                boxes = apiBoxes
                            });
                        }
                        catch (Exception ex)
                        {
                            transaction.Rollback();

                            _logger.LogError(ex, "Erro ao persistir o pedido {OriginalOrderId}. Transação revertida.", order.OrderId);

                            results.Add(new OrderPackingResultDTO
                            {
                                order_id = 0,
                                boxes = apiBoxes
                            });
                        }
                    }
                }
            }

            return results;
        }

        private OrderEntity ExecuteOrderPersistence(OrderDTO order, IEnumerable<PackedBoxDTO> packed, int userId)
        {
            var orderEntity = new OrderEntity
            {
                OrderDate = DateTime.UtcNow,
                UserId = userId,
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
            _dbContext.SaveChanges(); foreach (var packedBox in packed)
            {
                if (packedBox.BoxType == "N/A")
                {
                    _logger.LogWarning("Produto(s) {Products} não couberam em nenhuma caixa disponível para o pedido {OrderId}. Observação: {Observation}",
                        string.Join(", ", packedBox.Products),
                        orderEntity.OrderId,
                        packedBox.Observacao);


                    continue;
                }

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
                else
                {
                    _logger.LogError("BoxEntity com BoxType '{BoxType}' não foi encontrada no banco de dados para o pedido {OrderId}. As caixas deveriam estar previamente cadastradas.",
                        packedBox.BoxType,
                        orderEntity.OrderId);
                }
            }

            _dbContext.SaveChanges();

            return orderEntity;
        }
    }
}