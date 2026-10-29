namespace OrderManagement.Domain;

public sealed record OrderItemDraft(string ProductName, int Quantity, decimal UnitPrice);
public sealed record OrderCreation(Order Order, IReadOnlyList<OrderItem> Items);

public sealed class OrderFactory
{
    public OrderCreation Create(
        Customer customer,
        IEnumerable<OrderItemDraft> itemDrafts,
        string? discountCode = null,
        decimal discountAmount = 0)
    {
        ArgumentNullException.ThrowIfNull(customer);
        if (customer.Id == Guid.Empty)
            throw new DomainValidationException("A customer is required.");

        var drafts = ValidateDrafts(itemDrafts);
        var orderId = Guid.NewGuid();
        var items = CreateItems(orderId, drafts);
        var subtotal = items.Sum(item => item.LineTotal);
        var order = new Order(orderId, customer, DateTime.UtcNow, subtotal, discountCode, discountAmount);
        return new OrderCreation(order, items);
    }

    public IReadOnlyList<OrderItem> Update(
        Order order,
        IEnumerable<OrderItemDraft> itemDrafts,
        string? discountCode,
        decimal discountAmount)
    {
        ArgumentNullException.ThrowIfNull(order);
        var drafts = ValidateDrafts(itemDrafts);
        var items = CreateItems(order.Id, drafts);
        var subtotal = items.Sum(item => item.LineTotal);
        if (discountAmount < 0 || discountAmount > subtotal)
            throw new DomainValidationException("Discount must be between zero and the order subtotal.");

        order.UpdatePricing(subtotal, discountCode, discountAmount);
        return items;
    }

    private static OrderItemDraft[] ValidateDrafts(IEnumerable<OrderItemDraft> itemDrafts)
    {
        var drafts = itemDrafts?.ToArray() ?? [];
        if (drafts.Length == 0)
            throw new DomainValidationException("An order must contain at least one item.");
        if (drafts.Length > 100)
            throw new DomainValidationException("An order cannot contain more than 100 items.");
        return drafts;
    }

    private static List<OrderItem> CreateItems(Guid orderId, IEnumerable<OrderItemDraft> drafts) =>
        drafts.Select(draft => new OrderItem(
            Guid.NewGuid(), orderId, draft.ProductName, draft.Quantity, draft.UnitPrice)).ToList();
}
