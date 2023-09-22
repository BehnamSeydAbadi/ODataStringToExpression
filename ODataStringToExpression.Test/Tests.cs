using System.Linq.Expressions;

namespace ODataStringToExpression.Test;

public class Tests
{
    [Fact]
    public void product_price_gt_10()
    {
        var odataUrl = "Price gt 10";
        Expression<Func<Product, bool>> expectedExpression = p => p.Price > 10;

        var expression = odataUrl.ToExpression<Product>();

        expectedExpression.Body.ToString().Should().Be(expression.Body.ToString());
        expectedExpression.ReturnType.Should().Be(expression.ReturnType);
    }
}

