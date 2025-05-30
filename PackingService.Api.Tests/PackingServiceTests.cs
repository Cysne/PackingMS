using FluentAssertions;
using Moq;
using PackingService.Api.DTOs;
using PackingService.Api.Services;
using PackingService.Api.Strategies;
using PackingService.Api.Data;
using PackingService.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace PackingService.Api.Tests;

public class PackingServiceTests
{
    private readonly Mock<IPackingStrategy> _mockStrategy;
    private readonly Mock<PackingDbContext> _mockDbContext;
    private readonly List<BoxDTO> _availableBoxes;
    private readonly PackingService.Api.Services.PackingService _packingService;

    public PackingServiceTests()
    {
        _mockStrategy = new Mock<IPackingStrategy>();

        var options = new DbContextOptionsBuilder<PackingDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _mockDbContext = new Mock<PackingDbContext>(options);

        _availableBoxes = new List<BoxDTO>
        {
            new BoxDTO { BoxType = "Small", Height = 10, Width = 10, Length = 10 },
            new BoxDTO { BoxType = "Medium", Height = 20, Width = 20, Length = 20 },
            new BoxDTO { BoxType = "Large", Height = 30, Width = 30, Length = 30 }
        };

        _packingService = new PackingService.Api.Services.PackingService(
            _mockStrategy.Object,
            _availableBoxes,
            _mockDbContext.Object
        );
    }

    [Fact]
    public void PackOrders_WithValidOrders_ShouldReturnCorrectResults()
    {
        var orders = new List<OrderDTO>
        {
            new OrderDTO
            {
                OrderId = 1,
                Products = new List<ProductDTO>
                {
                    new ProductDTO { Name = "Product1", Height = 5, Width = 5, Length = 5 },
                    new ProductDTO { Name = "Product2", Height = 3, Width = 3, Length = 3 }
                }
            }
        };

        var packedBoxes = new List<PackedBoxDTO>
        {
            new PackedBoxDTO
            {
                BoxType = "Small",
                Products = new List<string> { "Product1", "Product2" },
                Observacao = null
            }
        };

        _mockStrategy.Setup(s => s.Pack(It.IsAny<IEnumerable<ProductDTO>>(), It.IsAny<IEnumerable<BoxDTO>>()))
                   .Returns(packedBoxes);

        var result = _packingService.PackOrders(orders);

        result.Should().NotBeEmpty();
        result.Should().HaveCount(1);
        result[0].order_id.Should().Be(1);
        result[0].boxes.Should().HaveCount(1);
        result[0].boxes[0].box_id.Should().Be("Small");
        result[0].boxes[0].products.Should().Contain("Product1");
        result[0].boxes[0].products.Should().Contain("Product2");
    }

    [Fact]
    public void PackOrders_WithMultipleOrders_ShouldReturnResultsForAllOrders()
    {
        var orders = new List<OrderDTO>
        {
            new OrderDTO
            {
                OrderId = 1,
                Products = new List<ProductDTO>
                {
                    new ProductDTO { Name = "Product1", Height = 5, Width = 5, Length = 5 }
                }
            },
            new OrderDTO
            {
                OrderId = 2,
                Products = new List<ProductDTO>
                {
                    new ProductDTO { Name = "Product2", Height = 15, Width = 15, Length = 15 }
                }
            }
        };

        _mockStrategy.SetupSequence(s => s.Pack(It.IsAny<IEnumerable<ProductDTO>>(), It.IsAny<IEnumerable<BoxDTO>>()))
                   .Returns(new List<PackedBoxDTO>
                   {
                       new PackedBoxDTO { BoxType = "Small", Products = new List<string> { "Product1" } }
                   })
                   .Returns(new List<PackedBoxDTO>
                   {
                       new PackedBoxDTO { BoxType = "Medium", Products = new List<string> { "Product2" } }
                   });

        var result = _packingService.PackOrders(orders);

        result.Should().HaveCount(2);
        result[0].order_id.Should().Be(1);
        result[1].order_id.Should().Be(2);
        result[0].boxes[0].box_id.Should().Be("Small");
        result[1].boxes[0].box_id.Should().Be("Medium");
    }

    [Fact]
    public void PackOrders_WithEmptyOrders_ShouldReturnEmptyResults()
    {
        var orders = new List<OrderDTO>();

        var result = _packingService.PackOrders(orders);

        result.Should().BeEmpty();
        _mockStrategy.Verify(s => s.Pack(It.IsAny<IEnumerable<ProductDTO>>(), It.IsAny<IEnumerable<BoxDTO>>()),
                           Times.Never);
    }

    [Fact]
    public void PackOrders_WithObservations_ShouldIncludeObservationsInResult()
    {
        var orders = new List<OrderDTO>
        {
            new OrderDTO
            {
                OrderId = 1,
                Products = new List<ProductDTO>
                {
                    new ProductDTO { Name = "LargeProduct", Height = 50, Width = 50, Length = 50 }
                }
            }
        };

        var packedBoxes = new List<PackedBoxDTO>
        {
            new PackedBoxDTO
            {
                BoxType = "N/A",
                Products = new List<string> { "LargeProduct" },
                Observacao = "Produto 'LargeProduct' não cabe em nenhuma das caixas disponíveis."
            }
        };

        _mockStrategy.Setup(s => s.Pack(It.IsAny<IEnumerable<ProductDTO>>(), It.IsAny<IEnumerable<BoxDTO>>()))
                   .Returns(packedBoxes);

        var result = _packingService.PackOrders(orders);

        result.Should().NotBeEmpty();
        result[0].boxes[0].observation.Should().Contain("não cabe em nenhuma das caixas disponíveis");
    }
}
