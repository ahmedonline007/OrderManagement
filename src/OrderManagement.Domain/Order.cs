namespace OrderManagement.Domain;

public sealed class Order
{
    private Order() { }

    internal Order(
        Guid id,
        Customer customer,
        DateTime createdAtUtc,
        decimal subtotal,
        string? discountCode,
        decimal discountAmount)
    {
        Id = id;
        Customer = customer;
        CustomerId = customer.Id;
        CreatedAtUtc = createdAtUtc;
        UpdatePricing(subtotal, discountCode, discountAmount);
    }

    public Guid Id { get; private set; }
    public Guid CustomerId { get; private set; }
    public Customer Customer { get; private set; } = null!;
    public DateTime CreatedAtUtc { get; private set; }
    public decimal Subtotal { get; private set; }
    public string? DiscountCode { get; private set; }
    public decimal DiscountAmount { get; private set; }
    public decimal Total => Subtotal - DiscountAmount;

    internal void UpdatePricing(decimal subtotal, string? code, decimal amount)
    {
        if (subtotal < 0)
            throw new DomainValidationException("Order subtotal cannot be negative.");
        if (amount < 0 || amount > subtotal)
            throw new DomainValidationException("Discount must be between zero and the order subtotal.");

        Subtotal = subtotal;
        DiscountCode = string.IsNullOrWhiteSpace(code) ? null : code.Trim();
        DiscountAmount = amount;
    }
}
