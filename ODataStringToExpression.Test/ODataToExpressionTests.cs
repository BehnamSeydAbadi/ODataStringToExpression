using Bogus;
using ODataStringToExpression.Test.Entities;
using ODataStringToExpression.Test.Entities.Inventory;
using ODataStringToExpression.Test.Entities.Product;

namespace ODataStringToExpression.Test;

public class ODataToExpressionTests
{
    [Fact]
    public void Price_eq_5()
    {
        var expecting = new ODataToExpression().Convert<ProductEntity>("?$filter=Price eq 5");

        Assert(
            expecting,
            expected: p => p.Price == 5,
            CreateProductEntity()
        );
    }

    [Fact]
    public void Price_ne_5()
    {
        var expecting = new ODataToExpression().Convert<ProductEntity>("?$filter=Price ne 5");

        Assert(
            expecting,
            expected: p => p.Price != 5,
            CreateProductEntity()
        );
    }

    [Fact]
    public void Price_gt_10()
    {
        var expecting = new ODataToExpression().Convert<ProductEntity>("?$filter=Price gt 10");

        Assert(
            expecting,
            expected: p => p.Price > 10,
            CreateProductEntity()
        );
    }

    [Fact]
    public void Price_ge_10()
    {
        var expecting = new ODataToExpression().Convert<ProductEntity>("?$filter=Price ge 10");

        Assert(
            expecting,
            expected: p => p.Price >= 10,
            CreateProductEntity()
        );
    }

    [Fact]
    public void Price_lt_20()
    {
        var expecting = new ODataToExpression().Convert<ProductEntity>("?$filter=Price lt 20");

        Assert(
            expecting,
            expected: p => p.Price < 20,
            CreateProductEntity()
        );
    }

    [Fact]
    public void Price_le_20()
    {
        var expecting = new ODataToExpression().Convert<ProductEntity>("?$filter=Price le 20");

        Assert(
            expecting,
            expected: p => p.Price <= 20,
            CreateProductEntity()
        );
    }

    [Fact]
    public void Price_gt_10_and_lt_20()
    {
        var expecting = new ODataToExpression().Convert<ProductEntity>("?$filter=Price gt 10 and Price lt 20");

        Assert(
            expecting,
            expected: p => p.Price > 10 && p.Price < 20,
            CreateProductEntity()
        );
    }

    [Fact]
    public void Price_gt_10_or_lt_20()
    {
        var expecting = new ODataToExpression().Convert<ProductEntity>("?$filter=Price gt 10 or Price lt 20");

        Assert(
            expecting,
            expected: p => p.Price > 10 || p.Price < 20,
            CreateProductEntity()
        );
    }

    [Fact]
    public void Status_eq_available()
    {
        var expecting = new ODataToExpression().Convert<ProductEntity>(
            $"?$filter=Status eq {(int)ProductStatus.Available}"
        );

        Assert(
            expecting,
            expected: p => p.Status == ProductStatus.Available,
            CreateProductEntity()
        );
    }

    [Fact]
    public void Price_gt_5_and_Price_le_20_Status_eq_Available()
    {
        var expecting = new ODataToExpression().Convert<ProductEntity>(
            $"?$filter=Price gt 5 and Price le 20 and Status eq {(int)ProductStatus.Available}"
        );

        Assert(
            expecting,
            expected: p => p.Price > 5 && p.Price < 20 && p.Status == ProductStatus.Available,
            CreateProductEntity()
        );
    }

    [Fact]
    public void CreateDate_eq_2014_06_26T03_30_00_000Z()
    {
        var expecting = new ODataToExpression().Convert<ProductEntity>(
            "?$filter=CreateDate eq 2014-06-26T03:30:00Z"
        );

        var dateTime = new DateTime(2014, 06, 26, 3, 30, 0);

        Assert(
            expecting,
            expected: p => p.CreateDate == dateTime,
            CreateProductEntity()
        );
    }

    [Fact]
    public void CreateDate_eq_2014_06_26()
    {
        var expecting = new ODataToExpression().Convert<ProductEntity>(
            "?$filter=CreateDate eq 2014-06-26"
        );

        var dateTime = new DateTime(2014, 06, 26);

        Assert(
            expecting,
            expected: p => p.CreateDate == dateTime,
            CreateProductEntity()
        );
    }

    [Fact]
    public void Status_in_1_2()
    {
        var expecting = new ODataToExpression().Convert<ProductEntity>(
            $"?$filter=Status in ('{(int)ProductStatus.Available}', '{(int)ProductStatus.SoldOut}')"
        );

        var status = new[] { ProductStatus.Available, ProductStatus.SoldOut };

        Assert(
            expecting,
            expected: p => status.Contains(p.Status),
            CreateProductEntity()
        );
    }

    [Fact]
    public void Status_not_in_1_2()
    {
        var expecting = new ODataToExpression().Convert<ProductEntity>(
            $"?$filter=not (Status in ('{(int)ProductStatus.Available}', '{(int)ProductStatus.SoldOut}'))"
        );

        var status = new[] { ProductStatus.Available, ProductStatus.SoldOut };

        Assert(
            expecting,
            expected: p => !status.Contains(p.Status),
            CreateProductEntity()
        );
    }

    [Fact]
    public void Price_gt_5_or_Status_eq_SoldOut()
    {
        var expecting = new ODataToExpression().Convert<ProductEntity>(
            $"?$filter=Price gt 5 or Status eq '{(int)ProductStatus.SoldOut}'"
        );

        Assert(
            expecting,
            expected: p => p.Price > 5 || p.Status == ProductStatus.SoldOut,
            CreateProductEntity()
        );
    }

    [Fact]
    public void Price_gt_5_and_Status_eq_Available_or_Price_le_20()
    {
        var expecting = new ODataToExpression().Convert<ProductEntity>(
            $"?$filter=Price gt 5 and (Status eq '{(int)ProductStatus.Available}' or Price le 20)"
        );

        Assert(
            expecting,
            expected: p => p.Price > 5 && (p.Status == ProductStatus.Available || p.Price <= 20),
            CreateProductEntity()
        );
    }

    [Fact]
    public void CreateDate_gt_2014_06_26T03_30_00_000Z_and_Status_eq_Available()
    {
        var expecting = new ODataToExpression().Convert<ProductEntity>(
            $"?$filter=CreateDate gt 2014-06-26T03:30:00.000Z and Status eq {(int)ProductStatus.Available}"
        );

        var dateTime = new DateTime(2014, 06, 26, 3, 30, 0);

        Assert(
            expecting,
            expected: p => p.CreateDate > dateTime && p.Status == ProductStatus.Available,
            CreateProductEntity()
        );
    }

    [Fact]
    public void Price_gt_5_and_Price_le_20_or_Status_eq_Available_and_CreateDate_eq_2014_06_26()
    {
        var expecting = new ODataToExpression().Convert<ProductEntity>(
            $"?$filter=Price gt 5 and Price le 20 or (Status eq {(int)ProductStatus.Available} and CreateDate eq 2014-06-26)"
        );

        var dateTime = new DateTime(2014, 06, 26);

        Assert(
            expecting,
            expected: p => p.Price > 5 && p.Price <= 20
                           || (p.Status == ProductStatus.Available && p.CreateDate == dateTime),
            CreateProductEntity()
        );
    }

    [Fact]
    public void Price_le_20_or_Status_in_Available_SoldOut_and_CreateDate_eq_2014_06_26()
    {
        var expecting = new ODataToExpression().Convert<ProductEntity>(
            $"?$filter=Price le 20 or (Status in ('{(int)ProductStatus.Available}', '{(int)ProductStatus.SoldOut}') and CreateDate eq 2014-06-26)"
        );

        var dateTime = new DateTime(2014, 06, 26);
        var status = new[] { ProductStatus.Available, ProductStatus.SoldOut };

        Assert(
            expecting,
            expected: p => p.Price <= 20
                           || (status.Contains(p.Status) && p.CreateDate == dateTime),
            CreateProductEntity()
        );
    }

    [Fact]
    public void Products_status_not_available_price_conditions_and_category()
    {
        var expecting = new ODataToExpression().Convert<ProductEntity>(
            $"?$filter=not (Status eq {(int)ProductStatus.NotAvailable}) and (Price gt 100 or Price le 50) and (Category in ('Electronics', 'Books'))"
        );

        var categories = new[] { ProductCategory.Electronics, ProductCategory.Books };

        Assert(
            expecting,
            expected: p => p.Status != ProductStatus.NotAvailable
                           && (p.Price > 100 || p.Price <= 50)
                           && categories.Contains(p.Category),
            CreateProductEntity()
        );
    }

    [Fact]
    public void Inventory_filter_products_any_price_equals_50()
    {
        var expecting = new ODataToExpression().Convert<InventoryEntity>(
            $"?$filter=Products/any(p: p/Price eq 50)"
        );

        Assert(
            expecting,
            expected: e => e.Products.Any(p => p.Price == 50),
            CreateInventoryEntity()
        );
    }

    [Fact]
    public void Inventory_filter_products_not_any_price_equals_50()
    {
        var expecting = new ODataToExpression().Convert<InventoryEntity>(
            $"?$filter=not Products/any(p: p/Price eq 50)"
        );

        Assert(
            expecting,
            expected: e => e.Products.Any(p => p.Price == 50) is false,
            CreateInventoryEntity()
        );
    }

    [Fact]
    public void Inventory_filter_products_all_price_gt_50()
    {
        var expecting = new ODataToExpression().Convert<InventoryEntity>(
            $"?$filter=Products/all(p: p/Price gt 50)"
        );

        Assert(
            expecting,
            expected: e => e.Products.All(p => p.Price > 50),
            CreateInventoryEntity()
        );
    }


    private static void Assert<TEntity>(Func<TEntity, bool> expecting, Func<TEntity, bool> expected,
        TEntity entityModel)
    {
        var expectingResult = expecting(entityModel);
        var expectedResult = expected(entityModel);

        expectingResult.Should().Be(expectedResult);
    }

    private ProductEntity CreateProductEntity(int? inventoryId = null)
    {
        return new Faker<ProductEntity>()
            .RuleFor(p => p.Id, f => f.Random.Int())
            .RuleFor(p => p.InventoryId, f => inventoryId is not null ? inventoryId.Value : f.Random.Int())
            .RuleFor(p => p.Price, f => f.Random.Decimal())
            .RuleFor(p => p.Status, f => f.PickRandom<ProductStatus>())
            .RuleFor(p => p.Category, f => f.PickRandom<ProductCategory>())
            .RuleFor(p => p.CreateDate, f => f.Date.Past())
            .Generate();
    }

    private InventoryEntity CreateInventoryEntity()
    {
        var inventoryEntity = new Faker<InventoryEntity>()
            .RuleFor(i => i.Id, f => f.Random.Int())
            .RuleFor(i => i.Name, f => f.Company.CompanyName())
            .RuleFor(i => i.Address, f => f.Address.FullAddress())
            .RuleFor(i => i.PhoneNumber, f => f.Phone.PhoneNumber())
            .RuleFor(i => i.Status, f => f.PickRandom<InventoryStatus>())
            .RuleFor(i => i.Products, new List<ProductEntity>())
            .Generate();

        for (var index = 0; index < new Random().Next(minValue: 1, maxValue: 100); index++)
        {
            inventoryEntity.Products.Add(
                CreateProductEntity(inventoryEntity.Id)
            );
        }

        return inventoryEntity;
    }
}