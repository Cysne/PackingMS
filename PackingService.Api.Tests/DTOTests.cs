using FluentAssertions;
using PackingService.Api.DTOs;

namespace PackingService.Api.Tests;

public class DTOTests
{
    [Fact]
    public void ProductDTO_ShouldHaveCorrectProperties()
    {
        var product = new ProductDTO
        {
            Name = "TestProduct",
            Height = 10.5m,
            Width = 5.5m,
            Length = 7.2m
        };

        product.Name.Should().Be("TestProduct");
        product.Height.Should().Be(10.5m);
        product.Width.Should().Be(5.5m);
        product.Length.Should().Be(7.2m);
    }

    [Fact]
    public void BoxDTO_ShouldHaveCorrectProperties()
    {
        var box = new BoxDTO
        {
            BoxType = "Large",
            Height = 20m,
            Width = 30m,
            Length = 40m
        };

        box.BoxType.Should().Be("Large");
        box.Height.Should().Be(20m);
        box.Width.Should().Be(30m);
        box.Length.Should().Be(40m);
    }

    [Fact]
    public void OrderDTO_ShouldHaveCorrectProperties()
    {
        var order = new OrderDTO
        {
            OrderId = 123,
            Products = new List<ProductDTO>
            {
                new ProductDTO { Name = "Product1", Height = 1, Width = 1, Length = 1 },
                new ProductDTO { Name = "Product2", Height = 2, Width = 2, Length = 2 }
            }
        };

        order.OrderId.Should().Be(123);
        order.Products.Should().HaveCount(2);
        order.Products[0].Name.Should().Be("Product1");
        order.Products[1].Name.Should().Be("Product2");
    }

    [Fact]
    public void PackedBoxDTO_ShouldHaveCorrectProperties()
    {
        var packedBox = new PackedBoxDTO
        {
            BoxType = "Medium",
            Products = new List<string> { "Product1", "Product2" },
            Observacao = "Test observation"
        };

        packedBox.BoxType.Should().Be("Medium");
        packedBox.Products.Should().HaveCount(2);
        packedBox.Products.Should().Contain("Product1");
        packedBox.Products.Should().Contain("Product2");
        packedBox.Observacao.Should().Be("Test observation");
    }

    [Fact]
    public void BoxResultDTO_ShouldHaveCorrectProperties()
    {
        var boxResult = new BoxResultDTO
        {
            box_id = "Small",
            products = new List<string> { "Product1" },
            observation = "Test observation"
        };

        boxResult.box_id.Should().Be("Small");
        boxResult.products.Should().HaveCount(1);
        boxResult.products.Should().Contain("Product1");
        boxResult.observation.Should().Be("Test observation");
    }

    [Fact]
    public void OrderPackingResultDTO_ShouldHaveCorrectProperties()
    {
        var orderResult = new OrderPackingResultDTO
        {
            order_id = 456,
            boxes = new List<BoxResultDTO>
            {
                new BoxResultDTO
                {
                    box_id = "Large",
                    products = new List<string> { "Product1", "Product2" },
                    observation = null
                }
            }
        };

        orderResult.order_id.Should().Be(456);
        orderResult.boxes.Should().HaveCount(1);
        orderResult.boxes[0].box_id.Should().Be("Large");
        orderResult.boxes[0].products.Should().HaveCount(2);
        orderResult.boxes[0].observation.Should().BeNull();
    }

    [Fact]
    public void PackedBoxDTO_ShouldInitializeProductsListByDefault()
    {
        var packedBox = new PackedBoxDTO();

        packedBox.Products.Should().NotBeNull();
        packedBox.Products.Should().BeEmpty();
    }

    [Fact]
    public void BoxResultDTO_ShouldInitializeProductsListByDefault()
    {
        var boxResult = new BoxResultDTO();

        boxResult.products.Should().NotBeNull();
        boxResult.products.Should().BeEmpty();
    }

    [Fact]
    public void OrderPackingResultDTO_ShouldInitializeBoxesListByDefault()
    {
        var orderResult = new OrderPackingResultDTO();

        orderResult.boxes.Should().NotBeNull();
        orderResult.boxes.Should().BeEmpty();
    }
}
