namespace OrderManagement.Domain;

public sealed class OrderItem
{
    private OrderItem() { }

    internal OrderItem(Guid id, Guid orderId, string productName, int quantity, decimal unitPrice)
    {
        if (string.IsNullOrWhiteSpace(productName))
            throw new DomainValidationException("Product name is required.");
        if (quantity <= 0)
            throw new DomainValidationException("Quantity must be greater than zero.");
        if (unitPrice <= 0)
            throw new DomainValidationException("Unit price must be greater than zero.");

        Id = id;
        OrderId = orderId;
        ProductName = productName.Trim();
        Quantity = quantity;
        UnitPrice = unitPrice;
    }

    public Guid Id { get; private set; }
    public Guid OrderId { get; private set; }
    public string ProductName { get; private set; } = string.Empty;
    public int Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }
    public decimal LineTotal => Quantity * UnitPrice;
}
