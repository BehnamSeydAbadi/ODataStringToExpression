using ODataStringToExpression.Test.Entities.Product;

namespace ODataStringToExpression.Test.Entities.Inventory;

public class InventoryEntity
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Address { get; set; }
    public string PhoneNumber { get; set; }
    public InventoryStatus Status { get; set; }

    public List<ProductEntity> Products { get; set; } = new();
}