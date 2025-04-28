namespace ODataStringToExpression.Test;

public class ODataToExpressionUsingMicrosoftOdataCoreTests
{
    [Fact]
    public void Price_eq_5()
    {
        var expecting = new ODataToExpressionUsingMicrosoftOdataCore<Product>().Convert("?$filter=Price eq 5");

        Assert(expecting, expected: p => p.Price == 5);
    }

    [Fact]
    public void Price_ne_5()
    {
        var expecting = new ODataToExpressionUsingMicrosoftOdataCore<Product>().Convert("?$filter=Price ne 5");

        Assert(expecting, expected: p => p.Price != 5);
    }

    [Fact]
    public void Price_gt_10()
    {
        var expecting = new ODataToExpressionUsingMicrosoftOdataCore<Product>().Convert("?$filter=Price gt 10");

        Assert(expecting, expected: p => p.Price > 10);
    }

    [Fact]
    public void Price_ge_10()
    {
        var expecting = new ODataToExpressionUsingMicrosoftOdataCore<Product>().Convert("?$filter=Price ge 10");

        Assert(expecting, expected: p => p.Price >= 10);
    }

    [Fact]
    public void Price_lt_20()
    {
        var expecting = new ODataToExpressionUsingMicrosoftOdataCore<Product>().Convert("?$filter=Price lt 20");

        Assert(expecting, expected: p => p.Price < 20);
    }

    [Fact]
    public void Price_le_20()
    {
        var expecting = new ODataToExpressionUsingMicrosoftOdataCore<Product>().Convert("?$filter=Price le 20");

        Assert(expecting, expected: p => p.Price <= 20);
    }

    [Fact]
    public void Price_gt_10_and_lt_20()
    {
        var expecting = new ODataToExpressionUsingMicrosoftOdataCore<Product>().Convert("?$filter=Price gt 10 and Price lt 20");

        Assert(expecting, expected: p => p.Price > 10 && p.Price < 20);
    }

    [Fact]
    public void Price_gt_10_or_lt_20()
    {
        var expecting = new ODataToExpressionUsingMicrosoftOdataCore<Product>().Convert("?$filter=Price gt 10 or Price lt 20");

        Assert(expecting, expected: p => p.Price > 10 || p.Price < 20);
    }

    [Fact]
    public void Status_eq_available()
    {
        var expecting = new ODataToExpressionUsingMicrosoftOdataCore<Product>().Convert(
            $"?$filter=Status eq {(int)ProductStatus.Available}"
        );

        Assert(expecting, expected: p => p.Status == ProductStatus.Available);
    }

    [Fact]
    public void Price_gt_5_and_Price_le_20_Status_eq_Available()
    {
        var expecting = new ODataToExpressionUsingMicrosoftOdataCore<Product>().Convert(
            $"?$filter=Price gt 5 and Price le 20 and Status eq {(int)ProductStatus.Available}"
        );

        Assert(expecting, expected: p => p.Price > 5 && p.Price < 20 && p.Status == ProductStatus.Available);
    }

    [Fact]
    public void CreateDate_eq_2014_06_26T03_30_00_000Z()
    {
        var expecting = new ODataToExpressionUsingMicrosoftOdataCore<Product>().Convert(
            "?$filter=CreateDate eq 2014-06-26T03:30:00.000Z"
        );

        var dateTime = new DateTime(2014, 06, 26, 3, 30, 0);

        Assert(expecting, expected: p => p.CreateDate == dateTime);
    }

    [Fact]
    public void CreateDate_eq_2014_06_26()
    {
        var expecting = new ODataToExpressionUsingMicrosoftOdataCore<Product>().Convert(
            "?$filter=CreateDate eq 2014-06-26"
        );

        var dateTime = new DateTime(2014, 06, 26);

        Assert(expecting, expected: p => p.CreateDate == dateTime);
    }

    [Fact]
    public void Status_in_1_2()
    {
        var expecting = new ODataToExpressionUsingMicrosoftOdataCore<Product>().Convert(
            $"?$filter=Status in ('{(int)ProductStatus.Available}', '{(int)ProductStatus.SoldOut}')"
        );

        var status = new[] { ProductStatus.Available, ProductStatus.SoldOut };

        Assert(expecting, expected: p => status.Contains(p.Status));
    }

    [Fact]
    public void Price_gt_5_or_Status_eq_SoldOut()
    {
        var expecting = new ODataToExpressionUsingMicrosoftOdataCore<Product>().Convert(
            $"?$filter=Price gt 5 or Status eq '{(int)ProductStatus.SoldOut}'"
        );

        Assert(expecting, expected: p => p.Price > 5 || p.Status == ProductStatus.SoldOut);
    }

    [Fact]
    public void Price_gt_5_and_Status_eq_Available_or_Price_le_20()
    {
        var expecting = new ODataToExpressionUsingMicrosoftOdataCore<Product>().Convert(
            $"?$filter=Price gt 5 and (Status eq '{(int)ProductStatus.Available}' or Price le 20)"
        );

        Assert(expecting, expected: p => p.Price > 5 && (p.Status == ProductStatus.Available || p.Price <= 20));
    }

    [Fact]
    public void CreateDate_gt_2014_06_26T03_30_00_000Z_and_Status_eq_Available()
    {
        var expecting = new ODataToExpressionUsingMicrosoftOdataCore<Product>().Convert(
            $"?$filter=CreateDate gt 2014-06-26T03:30:00.000Z and Status eq {(int)ProductStatus.Available}"
        );

        var dateTime = new DateTime(2014, 06, 26, 3, 30, 0);

        Assert(expecting, expected: p => p.CreateDate > dateTime && p.Status == ProductStatus.Available);
    }

    [Fact]
    public void Price_gt_5_and_Price_le_20_or_Status_eq_Available_and_CreateDate_eq_2014_06_26()
    {
        var expecting = new ODataToExpressionUsingMicrosoftOdataCore<Product>().Convert(
            $"?$filter=Price gt 5 and Price le 20 or (Status eq {(int)ProductStatus.Available} and CreateDate eq 2014-06-26)"
        );
        
        var dateTime = new DateTime(2014, 06, 26);

        Assert(expecting, expected: p => p.Price > 5 && p.Price <= 20
                                         || (p.Status == ProductStatus.Available && p.CreateDate == dateTime));
    }

    // [Fact]
    // public void Price_le_20_or_Status_in_Available_SoldOut_and_CreateDate_eq_2014_06_26()
    // {
    //     var odataUrl =
    //         $"Price le 20 or (Status in ({(int)ProductStatus.Available}, {(int)ProductStatus.SoldOut}) and CreateDate eq 2014-06-26)";
    //
    //     var expecting = new ODataToExpression<Product>().Convert(odataUrl);
    //
    //     var dateTime = new DateTime(2014, 06, 26);
    //     var status = new[] { ProductStatus.Available, ProductStatus.SoldOut };
    //
    //     Assert(expecting, expected: p => p.Price <= 20
    //                                      || (status.Contains(p.Status) && p.CreateDate == dateTime));
    // }

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