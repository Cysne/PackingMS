using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using PackingService.Api.DTOs;
using PackingService.Api.Services;

namespace PackingService.Api.Tests;

public class PackingControllerTests
{
    private readonly Mock<IPackingService> _mockPackingService;
    private readonly Mock<ILogger<PackingController>> _mockLogger;
    private readonly PackingController _controller;

    public PackingControllerTests()
    {
        _mockPackingService = new Mock<IPackingService>();
        _mockLogger = new Mock<ILogger<PackingController>>();
        _controller = new PackingController(_mockPackingService.Object, _mockLogger.Object);
    }

    [Fact]
    public void PackOrders_WithValidOrders_ShouldReturnOkResult()
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
            }
        };

        var expectedResult = new List<OrderPackingResultDTO>
        {
            new OrderPackingResultDTO
            {
                order_id = 1,
                boxes = new List<BoxResultDTO>
                {
                    new BoxResultDTO
                    {
                        box_id = "Small",
                        products = new List<string> { "Product1" },
                        observation = null
                    }
                }
            }
        };

        _mockPackingService.Setup(s => s.PackOrders(It.IsAny<IEnumerable<OrderDTO>>()))
                          .Returns(expectedResult);

        var result = _controller.PackOrders(orders);

        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult?.Value.Should().BeEquivalentTo(expectedResult);
    }
    [Fact]
    public void PackOrders_WithNullOrders_ShouldReturnBadRequest()
    {
        var result = _controller.PackOrders(null!);

        result.Should().BeOfType<BadRequestObjectResult>();
        var badRequestResult = result as BadRequestObjectResult;
        badRequestResult?.Value.Should().Be("No orders provided.");
    }

    [Fact]
    public void PackOrders_WithEmptyOrders_ShouldReturnBadRequest()
    {
        var orders = new List<OrderDTO>();

        var result = _controller.PackOrders(orders);

        result.Should().BeOfType<BadRequestObjectResult>();
        var badRequestResult = result as BadRequestObjectResult;
        badRequestResult?.Value.Should().Be("No orders provided.");
    }

    [Fact]
    public void PackOrders_WhenServiceThrowsException_ShouldReturnInternalServerError()
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
            }
        };

        var exception = new Exception("Test exception");
        _mockPackingService.Setup(s => s.PackOrders(It.IsAny<IEnumerable<OrderDTO>>()))
                          .Throws(exception);

        var result = _controller.PackOrders(orders);

        result.Should().BeOfType<ObjectResult>();
        var objectResult = result as ObjectResult;
        objectResult?.StatusCode.Should().Be(500);
        objectResult?.Value.Should().Be("Internal server error: Test exception");
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Erro ao processar o pedido no endpoint pack-orders")),
                exception,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void PackOrders_WithComplexOrders_ShouldCallServiceWithCorrectParameters()
    {
        var orders = new List<OrderDTO>
        {
            new OrderDTO
            {
                OrderId = 1,
                Products = new List<ProductDTO>
                {
                    new ProductDTO { Name = "Product1", Height = 5, Width = 5, Length = 5 },
                    new ProductDTO { Name = "Product2", Height = 10, Width = 10, Length = 10 }
                }
            },
            new OrderDTO
            {
                OrderId = 2,
                Products = new List<ProductDTO>
                {
                    new ProductDTO { Name = "Product3", Height = 15, Width = 15, Length = 15 }
                }
            }
        };

        var expectedResult = new List<OrderPackingResultDTO>();
        _mockPackingService.Setup(s => s.PackOrders(It.IsAny<IEnumerable<OrderDTO>>()))
                          .Returns(expectedResult);

        var result = _controller.PackOrders(orders);

        _mockPackingService.Verify(s => s.PackOrders(
            It.Is<IEnumerable<OrderDTO>>(o =>
                o.Count() == 2 &&
                o.First().OrderId == 1 &&
                o.Last().OrderId == 2)),
            Times.Once);
    }
}
