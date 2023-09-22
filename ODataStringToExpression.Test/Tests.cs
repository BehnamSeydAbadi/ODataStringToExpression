using System.Linq.Expressions;

namespace ODataStringToExpression.Test;

public class Tests
{
    [Fact]
    public void price_gt_10()
    {
        var odataUrl = "Price gt 10";

        var expecting = odataUrl.ToExpression<Product>();

        Assert(expecting, expected: p => p.Price > 10);
    }

    [Fact]
    public void price_eq_10()
    {
        var odataUrl = "Price eq 10";

        var expecting = odataUrl.ToExpression<Product>();

        Assert(expecting, expected: p => p.Price == 10);
    }

    [Fact]
    public void price_lt_20()
    {
        var odataUrl = "Price lt 20";

        var expecting = odataUrl.ToExpression<Product>();

        Assert(expecting, expected: p => p.Price < 20);
    }


    private void Assert(
            Expression<Func<Product, bool>> expecting,
            Expression<Func<Product, bool>> expected)
    {
        expecting.Body.ToString().Should().Be(expected.Body.ToString());
        expecting.ReturnType.Should().Be(expected.ReturnType);
    }
}

