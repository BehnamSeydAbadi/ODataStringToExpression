using System.Linq.Expressions;

namespace ODataStringToExpression.Test;

public class Tests
{
    [Fact]
    public void Price_eq_5()
    {
        var odataUrl = "Price eq 5";

        var expecting = new ODataToExpression().Convert<Product>(odataUrl);

        Assert(expecting, expected: p => p.Price == 5);
    }

    [Fact]
    public void Price_ne_5()
    {
        var odataUrl = "Price ne 5";

        var expecting = new ODataToExpression().Convert<Product>(odataUrl);

        Assert(expecting, expected: p => p.Price != 5);
    }

    [Fact]
    public void Price_gt_10()
    {
        var odataUrl = "Price gt 10";

        var expecting = new ODataToExpression().Convert<Product>(odataUrl);

        Assert(expecting, expected: p => p.Price > 10);
    }

    [Fact]
    public void Price_ge_10()
    {
        var odataUrl = "Price ge 10";

        var expecting = new ODataToExpression().Convert<Product>(odataUrl);

        Assert(expecting, expected: p => p.Price >= 10);
    }

    [Fact]
    public void Price_lt_20()
    {
        var odataUrl = "Price lt 20";

        var expecting = new ODataToExpression().Convert<Product>(odataUrl);

        Assert(expecting, expected: p => p.Price < 20);
    }

    [Fact]
    public void Price_le_20()
    {
        var odataUrl = "Price le 20";

        var expecting = new ODataToExpression().Convert<Product>(odataUrl);

        Assert(expecting, expected: p => p.Price <= 20);
    }

    [Fact]
    public void Price_gt_10_and_lt_20()
    {
        var odataUrl = "Price gt 10 and Price lt 20";

        var expecting = new ODataToExpression().Convert<Product>(odataUrl);

        Assert(expecting, expected: p => p.Price > 10 && p.Price < 20);
    }

    [Fact]
    public void Price_gt_10_or_lt_20()
    {
        var odataUrl = "Price gt 10 or Price lt 20";

        var expecting = new ODataToExpression().Convert<Product>(odataUrl);

        Assert(expecting, expected: p => p.Price > 10 || p.Price < 20);
    }

    [Fact]
    public void Status_eq_available()
    {
        var odataUrl = $"Status eq {(int)ProductStatus.Available}";

        var expecting = new ODataToExpression().Convert<Product>(odataUrl);

        Assert(expecting, expected: p => p.Status == ProductStatus.Available);
    }

    private void Assert(
            Func<Product, bool> expecting,
            Func<Product, bool> expected)
    {
        var product = new Product { Price = 10 };

        var expectingResult = expecting(product);
        var expectedResult = expected(product);

        expectingResult.Should().Be(expectedResult);
    }
}
