namespace ODataStringToExpression.Test
{
    internal class Product
    {
        public int Id { get; set; }
        public decimal Price { get; set; }
        public ProductStatus Status { get; set; }
        public ProductCategory Category { get; set; }
        public DateTime CreateDate { get; set; }
    }

    internal enum ProductStatus
    {
        SoldOut = 1,
        Available,
        NotAvailable
    }

    internal enum ProductCategory
    {
        Electronics = 1,
        Books
    }
}