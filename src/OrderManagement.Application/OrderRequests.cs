using MediatR;
using System.ComponentModel.DataAnnotations;

namespace OrderManagement.Application;

public sealed record CreateOrderItemRequest(
    [param: Required, StringLength(200, MinimumLength = 1)] string ProductName,
    [param: Range(1, 100000)] int Quantity,
    [param: Range(typeof(decimal), "0.01", "100000000")] decimal UnitPrice);

public sealed record CreateOrderRequest(
    [param: Required, StringLength(200, MinimumLength = 1)] string CustomerName,
    [param: Required, EmailAddress, StringLength(320)] string CustomerEmail,
    [param: Required, MinLength(1), MaxLength(100)] List<CreateOrderItemRequest> Items,
    string? DiscountStrategy);

public sealed record UpdateOrderRequest(
    [param: Required, MinLength(1), MaxLength(100)] List<CreateOrderItemRequest> Items,
    string? DiscountStrategy);

public sealed record CreateOrderCommand(CreateOrderRequest Data) : IRequest<OrderResponse>;
public sealed record UpdateOrderCommand(Guid Id, UpdateOrderRequest Data) : IRequest<OrderResponse?>;
public sealed record GetOrdersQuery(OrderSpecification Specification) : IRequest<PagedOrdersResponse>;
public sealed record GetOrderQuery(Guid Id) : IRequest<OrderResponse?>;
public sealed record DeleteOrderCommand(Guid Id) : IRequest<bool>;
