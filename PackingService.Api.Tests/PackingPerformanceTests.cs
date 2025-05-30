using FluentAssertions;
using PackingService.Api.DTOs;
using PackingService.Api.Strategies;
using System.Diagnostics;

namespace PackingService.Api.Tests;

public class PackingPerformanceTests
{
    [Fact]
    public void Pack_WithLargeNumberOfProducts_ShouldCompleteInReasonableTime()
    {
        var strategy = new FirstFitDecreasingPackingStrategy();
        var products = GenerateProducts(1000);
        var boxes = GenerateBoxes();

        var stopwatch = new Stopwatch();

        stopwatch.Start();
        var result = strategy.Pack(products, boxes);
        stopwatch.Stop();

        stopwatch.ElapsedMilliseconds.Should().BeLessThan(5000);
        result.Should().NotBeEmpty();
    }
    [Fact]
    public void Pack_WithMixedSizedProducts_ShouldOptimizeBoxUsage()
    {
        var strategy = new FirstFitDecreasingPackingStrategy();

        var products = new List<ProductDTO>
        {
            new ProductDTO { Name = "Small1", Height = 2, Width = 2, Length = 2 },
            new ProductDTO { Name = "Small2", Height = 2, Width = 2, Length = 2 },
            new ProductDTO { Name = "Small3", Height = 2, Width = 2, Length = 2 },
            new ProductDTO { Name = "Small4", Height = 2, Width = 2, Length = 2 },

            new ProductDTO { Name = "Medium1", Height = 8, Width = 8, Length = 8 },

            new ProductDTO { Name = "Large1", Height = 15, Width = 15, Length = 15 }
        };

        var boxes = new List<BoxDTO>
        {
            new BoxDTO { BoxType = "Small", Height = 5, Width = 5, Length = 5 },
            new BoxDTO { BoxType = "Medium", Height = 10, Width = 10, Length = 10 },
            new BoxDTO { BoxType = "Large", Height = 20, Width = 20, Length = 20 }
        };

        var result = strategy.Pack(products, boxes);

        result.Should().NotBeEmpty();

        var totalProductsInBoxes = result.SelectMany(r => r.Products).Count();
        totalProductsInBoxes.Should().Be(products.Count);
        var mediumBoxes = result.Where(r => r.BoxType == "Medium").ToList();
        var largeBoxes = result.Where(r => r.BoxType == "Large").ToList();
        var smallBoxes = result.Where(r => r.BoxType == "Small").ToList();

        var nonSmallBoxes = mediumBoxes.Concat(largeBoxes).ToList();
        nonSmallBoxes.Should().NotBeEmpty("deveria usar caixas maiores para produtos grandes");
    }
    [Fact]
    public void Pack_WithIdenticalProducts_ShouldDistributeEvenly()
    {
        var strategy = new FirstFitDecreasingPackingStrategy();

        var products = Enumerable.Range(1, 20)
            .Select(i => new ProductDTO { Name = $"Product{i}", Height = 5, Width = 5, Length = 5 })
            .ToList();

        var boxes = new List<BoxDTO>
        {
            new BoxDTO { BoxType = "Standard", Height = 10, Width = 10, Length = 10 }
        };

        var result = strategy.Pack(products, boxes);

        result.Should().NotBeEmpty();

        var totalProductsInBoxes = result.SelectMany(r => r.Products).Count();
        totalProductsInBoxes.Should().Be(products.Count);

        result.Should().HaveCountGreaterThan(1);

        result.Should().AllSatisfy(box => box.Products.Should().NotBeEmpty());
    }

    [Fact]
    public void Pack_WithEdgeCaseDimensions_ShouldHandleCorrectly()
    {
        var strategy = new FirstFitDecreasingPackingStrategy();

        var products = new List<ProductDTO>
        {
            new ProductDTO { Name = "Thin", Height = 0.1m, Width = 10, Length = 10 },

            new ProductDTO { Name = "Cube", Height = 5, Width = 5, Length = 5 },

            new ProductDTO { Name = "Long", Height = 1, Width = 1, Length = 15 }
        };

        var boxes = new List<BoxDTO>
        {
            new BoxDTO { BoxType = "Flexible", Height = 12, Width = 12, Length = 20 }
        };

        var result = strategy.Pack(products, boxes);

        result.Should().NotBeEmpty();
        result.Should().HaveCount(1);
        result[0].Products.Should().HaveCount(3);
        result[0].Observacao.Should().BeNull();
    }

    private static List<ProductDTO> GenerateProducts(int count)
    {
        var random = new Random(42);
        var products = new List<ProductDTO>();

        for (int i = 0; i < count; i++)
        {
            products.Add(new ProductDTO
            {
                Name = $"Product{i}",
                Height = (decimal)(random.NextDouble() * 10 + 1),
                Width = (decimal)(random.NextDouble() * 10 + 1),
                Length = (decimal)(random.NextDouble() * 10 + 1)
            });
        }

        return products;
    }

    private static List<BoxDTO> GenerateBoxes()
    {
        return new List<BoxDTO>
        {
            new BoxDTO { BoxType = "XSmall", Height = 5, Width = 5, Length = 5 },
            new BoxDTO { BoxType = "Small", Height = 10, Width = 10, Length = 10 },
            new BoxDTO { BoxType = "Medium", Height = 15, Width = 15, Length = 15 },
            new BoxDTO { BoxType = "Large", Height = 20, Width = 20, Length = 20 },
            new BoxDTO { BoxType = "XLarge", Height = 30, Width = 30, Length = 30 }
        };
    }
}
