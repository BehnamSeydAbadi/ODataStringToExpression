namespace ODataStringToExpression.Test;

public class ODataToExpressionUsingMicrosoftOdataCoreTests
{
    [Fact]
    public void Price_eq_5()
    {
        var expecting = new ODataToExpressionUsingMicrosoftOdataCore<Product>().Convert("?$filter=Price eq 5");

        Assert(expecting, expected: p => p.Price == 5);
    }

    private static void Assert(Func<Product, bool> expecting, Func<Product, bool> expected)
    {
        var product = new Product
        {
            Price = 10,
            Status = ProductStatus.Available,
            CreateDate = DateTime.Now,
        };

        var expectingResult = expecting(product);
        var expectedResult = expected(product);

        expectingResult.Should().Be(expectedResult);
    }
}