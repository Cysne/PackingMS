using FluentAssertions;
using Moq;
using PackingService.Api.DTOs;
using PackingService.Api.Services;
using PackingService.Api.Strategies;
using PackingService.Api.Data;
using Microsoft.Extensions.Logging;

namespace PackingService.Api.Tests;

public class FirstFitDecreasingPackingStrategyTests
{
    [Fact]
    public void Pack_WithSimpleProducts_ShouldPackInCorrectBoxes()
    {
        var strategy = new FirstFitDecreasingPackingStrategy();

        var products = new List<ProductDTO>
        {
            new ProductDTO { Name = "Product1", Height = 5, Width = 5, Length = 5 },
            new ProductDTO { Name = "Product2", Height = 3, Width = 3, Length = 3 }
        };

        var boxes = new List<BoxDTO>
        {
            new BoxDTO { BoxType = "Small", Height = 10, Width = 10, Length = 10 },
            new BoxDTO { BoxType = "Large", Height = 20, Width = 20, Length = 20 }
        };

        var result = strategy.Pack(products, boxes);

        result.Should().NotBeEmpty();
        result.Should().HaveCount(1);
        result[0].BoxType.Should().Be("Small");
        result[0].Products.Should().Contain("Product1");
        result[0].Products.Should().Contain("Product2");
        result[0].Observacao.Should().BeNull();
    }

    [Fact]
    public void Pack_WithProductThatDoesNotFit_ShouldCreateObservation()
    {
        var strategy = new FirstFitDecreasingPackingStrategy();

        var products = new List<ProductDTO>
        {
            new ProductDTO { Name = "LargeProduct", Height = 50, Width = 50, Length = 50 }
        };

        var boxes = new List<BoxDTO>
        {
            new BoxDTO { BoxType = "Small", Height = 10, Width = 10, Length = 10 }
        };

        var result = strategy.Pack(products, boxes);

        result.Should().NotBeEmpty();
        result.Should().HaveCount(1);
        result[0].BoxType.Should().Be("N/A");
        result[0].Products.Should().Contain("LargeProduct");
        result[0].Observacao.Should().Contain("não cabe em nenhuma das caixas disponíveis");
    }

    [Fact]
    public void Pack_WithMultipleProductsRequiringMultipleBoxes_ShouldUseMultipleBoxes()
    {
        var strategy = new FirstFitDecreasingPackingStrategy();

        var products = new List<ProductDTO>
        {
            new ProductDTO { Name = "Product1", Height = 8, Width = 8, Length = 8 },
            new ProductDTO { Name = "Product2", Height = 8, Width = 8, Length = 8 },
            new ProductDTO { Name = "Product3", Height = 2, Width = 2, Length = 2 }
        };

        var boxes = new List<BoxDTO>
        {
            new BoxDTO { BoxType = "Medium", Height = 10, Width = 10, Length = 10 }
        };

        var result = strategy.Pack(products, boxes);
        result.Should().HaveCount(2);


        var allProducts = result.SelectMany(r => r.Products).ToList();
        allProducts.Should().HaveCount(3);
        allProducts.Should().Contain("Product1");
        allProducts.Should().Contain("Product2");
        allProducts.Should().Contain("Product3");
    }

    [Fact]
    public void Pack_WithEmptyProducts_ShouldReturnEmptyResult()
    {
        var strategy = new FirstFitDecreasingPackingStrategy();
        var products = new List<ProductDTO>();
        var boxes = new List<BoxDTO>
        {
            new BoxDTO { BoxType = "Small", Height = 10, Width = 10, Length = 10 }
        };

        var result = strategy.Pack(products, boxes);

        result.Should().BeEmpty();
    }
}
