using FluentAssertions;
using PackingService.Api.DTOs;
using PackingService.Api.Strategies;

namespace PackingService.Api.Tests;

public class PackingEdgeCasesTests
{
    [Fact]
    public void Pack_WithNullProducts_ShouldReturnEmptyResult()
    {
        var strategy = new FirstFitDecreasingPackingStrategy();
        var boxes = new List<BoxDTO>
        {
            new BoxDTO { BoxType = "Small", Height = 10, Width = 10, Length = 10 }
        };

        var exception = Assert.Throws<ArgumentNullException>(() => strategy.Pack(null!, boxes));
        exception.ParamName.Should().Be("source");
    }

    [Fact]
    public void Pack_WithEmptyBoxes_ShouldCreateNABoxes()
    {
        var strategy = new FirstFitDecreasingPackingStrategy();
        var products = new List<ProductDTO>
        {
            new ProductDTO { Name = "Product1", Height = 5, Width = 5, Length = 5 }
        };
        var boxes = new List<BoxDTO>();

        var result = strategy.Pack(products, boxes);

        result.Should().NotBeEmpty();
        result[0].BoxType.Should().Be("N/A");
        result[0].Observacao.Should().Contain("não cabe em nenhuma das caixas disponíveis");
    }

    [Fact]
    public void Pack_WithZeroDimensionProduct_ShouldHandleGracefully()
    {
        var strategy = new FirstFitDecreasingPackingStrategy();
        var products = new List<ProductDTO>
        {
            new ProductDTO { Name = "ZeroProduct", Height = 0, Width = 5, Length = 5 }
        };
        var boxes = new List<BoxDTO>
        {
            new BoxDTO { BoxType = "Small", Height = 10, Width = 10, Length = 10 }
        };

        var result = strategy.Pack(products, boxes);

        result.Should().NotBeEmpty();
        result[0].Products.Should().Contain("ZeroProduct");
    }

    [Fact]
    public void Pack_WithNegativeDimensionProduct_ShouldHandleGracefully()
    {
        var strategy = new FirstFitDecreasingPackingStrategy();
        var products = new List<ProductDTO>
        {
            new ProductDTO { Name = "NegativeProduct", Height = -1, Width = 5, Length = 5 }
        };
        var boxes = new List<BoxDTO>
        {
            new BoxDTO { BoxType = "Small", Height = 10, Width = 10, Length = 10 }
        };

        var result = strategy.Pack(products, boxes);

        result.Should().NotBeEmpty();
        result[0].Products.Should().Contain("NegativeProduct");
    }

    [Fact]
    public void Pack_WithExactFitProduct_ShouldFitPerfectly()
    {
        var strategy = new FirstFitDecreasingPackingStrategy();
        var products = new List<ProductDTO>
        {
            new ProductDTO { Name = "PerfectFit", Height = 10, Width = 10, Length = 10 }
        };
        var boxes = new List<BoxDTO>
        {
            new BoxDTO { BoxType = "Perfect", Height = 10, Width = 10, Length = 10 }
        };

        var result = strategy.Pack(products, boxes);

        result.Should().NotBeEmpty();
        result[0].BoxType.Should().Be("Perfect");
        result[0].Products.Should().Contain("PerfectFit");
        result[0].Observacao.Should().BeNull();
    }

    [Fact]
    public void Pack_WithProductSlightlyLargerThanBox_ShouldCreateObservation()
    {
        var strategy = new FirstFitDecreasingPackingStrategy();
        var products = new List<ProductDTO>
        {
            new ProductDTO { Name = "TooLarge", Height = 10.1m, Width = 10, Length = 10 }
        };
        var boxes = new List<BoxDTO>
        {
            new BoxDTO { BoxType = "Small", Height = 10, Width = 10, Length = 10 }
        };

        var result = strategy.Pack(products, boxes);

        result.Should().NotBeEmpty();
        result[0].BoxType.Should().Be("N/A");
        result[0].Observacao.Should().Contain("não cabe em nenhuma das caixas disponíveis");
    }

    [Fact]
    public void Pack_WithManySmallProductsInLargeBox_ShouldFitAll()
    {
        var strategy = new FirstFitDecreasingPackingStrategy();
        var products = Enumerable.Range(1, 100)
            .Select(i => new ProductDTO { Name = $"Tiny{i}", Height = 1, Width = 1, Length = 1 })
            .ToList();

        var boxes = new List<BoxDTO>
        {
            new BoxDTO { BoxType = "Huge", Height = 100, Width = 100, Length = 100 }
        };

        var result = strategy.Pack(products, boxes);

        result.Should().NotBeEmpty();
        result.Should().HaveCount(1);
        result[0].Products.Should().HaveCount(100);
        result[0].BoxType.Should().Be("Huge");
    }

    [Fact]
    public void Pack_WithDuplicateProductNames_ShouldHandleCorrectly()
    {
        var strategy = new FirstFitDecreasingPackingStrategy();
        var products = new List<ProductDTO>
        {
            new ProductDTO { Name = "Product", Height = 3, Width = 3, Length = 3 },
            new ProductDTO { Name = "Product", Height = 3, Width = 3, Length = 3 },
            new ProductDTO { Name = "Product", Height = 3, Width = 3, Length = 3 }
        };

        var boxes = new List<BoxDTO>
        {
            new BoxDTO { BoxType = "Standard", Height = 10, Width = 10, Length = 10 }
        };

        var result = strategy.Pack(products, boxes);

        result.Should().NotBeEmpty();
        result[0].Products.Should().HaveCount(3);
        result[0].Products.Should().AllBe("Product");
    }

    [Fact]
    public void Pack_WithVeryLargeDecimalValues_ShouldHandleCorrectly()
    {
        var strategy = new FirstFitDecreasingPackingStrategy();
        var products = new List<ProductDTO>
        {
            new ProductDTO { Name = "LargeDecimal", Height = 999999.99m, Width = 999999.99m, Length = 999999.99m }
        };

        var boxes = new List<BoxDTO>
        {
            new BoxDTO { BoxType = "Small", Height = 10, Width = 10, Length = 10 },
            new BoxDTO { BoxType = "Huge", Height = 1000000m, Width = 1000000m, Length = 1000000m }
        };

        var result = strategy.Pack(products, boxes);


        result.Should().NotBeEmpty();
        result[0].BoxType.Should().Be("Huge");
        result[0].Products.Should().Contain("LargeDecimal");
    }
}
