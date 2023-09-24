namespace ODataStringToExpression.Test
{
    internal class Product
    {
        public decimal Price { get; set; }
        public ProductStatus Status { get; set; }
    }

    internal enum ProductStatus
    {
        SoldOut = 1,
        Available
    }
}
