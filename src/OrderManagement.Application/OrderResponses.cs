using OrderManagement.Domain;

namespace OrderManagement.Application;

public sealed record OrderItemResponse(Guid Id, string ProductName, int Quantity, decimal UnitPrice, decimal LineTotal);
public sealed record CustomerResponse(Guid Id, string Name, string Email);
public sealed record OrderResponse(
    Guid Id,
    DateTime CreatedAtUtc,
    CustomerResponse Customer,
    IReadOnlyList<OrderItemResponse> Items,
    decimal Subtotal,
    string? DiscountCode,
    decimal DiscountAmount,
    decimal Total);
public sealed record PagedOrdersResponse(IReadOnlyList<OrderResponse> Items, int Page, int PageSize, int TotalCount);

public static class OrderResponseMapper
{
    public static OrderResponse ToResponse(Order order, IEnumerable<OrderItem> items) => new(
        order.Id,
        order.CreatedAtUtc,
        new CustomerResponse(order.Customer.Id, order.Customer.Name, order.Customer.Email),
        items.Select(item => new OrderItemResponse(
            item.Id, item.ProductName, item.Quantity, item.UnitPrice, item.LineTotal)).ToArray(),
        order.Subtotal,
        order.DiscountCode,
        order.DiscountAmount,
        order.Total);
}
