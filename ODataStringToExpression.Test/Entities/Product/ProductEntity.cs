using ODataStringToExpression.Test.Entities.Inventory;

namespace ODataStringToExpression.Test.Entities.Product
{
    public class ProductEntity
    {
        public int Id { get; set; }
        public int InventoryId { get; set; }
        public decimal Price { get; set; }
        public ProductStatus Status { get; set; }
        public ProductCategory Category { get; set; }
        public DateTime CreateDate { get; set; }

        public InventoryEntity InventoryEntity { get; set; }
    }
}