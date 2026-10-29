using MediatR;
using OrderManagement.Domain;

namespace OrderManagement.Application;

public sealed class CreateOrderHandler(
    IOrderRepository orders,
    ICustomerRepository customers,
    IOrderItemRepository orderItems,
    OrderFactory factory,
    DiscountStrategyResolver discounts,
    IUnitOfWork unitOfWork) : IRequestHandler<CreateOrderCommand, OrderResponse>
{
    public async Task<OrderResponse> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        var data = request.Data;
        var customer = await customers.FindByEmailAsync(data.CustomerEmail.Trim(), cancellationToken);
        if (customer is null)
        {
            customer = new Customer(Guid.NewGuid(), data.CustomerName, data.CustomerEmail);
            customers.Add(customer);
        }

        var drafts = data.Items.Select(item => new OrderItemDraft(item.ProductName, item.Quantity, item.UnitPrice)).ToArray();
        var subtotal = drafts.Sum(item => item.Quantity * item.UnitPrice);
        var discountCode = string.IsNullOrWhiteSpace(data.DiscountStrategy) ? null : data.DiscountStrategy.Trim();
        var discountAmount = discountCode is null
            ? 0
            : discounts.Resolve(discountCode).CalculateDiscount(subtotal);
        var creation = factory.Create(customer, drafts, discountCode, discountAmount);
        orders.Add(creation.Order);
        orderItems.AddRange(creation.Items);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return OrderResponseMapper.ToResponse(creation.Order, creation.Items);
    }
}

public sealed class GetOrdersHandler(IOrderRepository orders, IOrderItemRepository orderItems)
    : IRequestHandler<GetOrdersQuery, PagedOrdersResponse>
{
    public async Task<PagedOrdersResponse> Handle(GetOrdersQuery request, CancellationToken cancellationToken)
    {
        var result = await orders.ListAsync(request.Specification, cancellationToken);
        var items = await orderItems.ListByOrderIdsAsync(
            result.Orders.Select(order => order.Id).ToArray(),
            cancellationToken);
        var itemsByOrder = items.GroupBy(item => item.OrderId)
            .ToDictionary(group => group.Key, group => (IEnumerable<OrderItem>)group.ToArray());
        return new PagedOrdersResponse(
            result.Orders.Select(order => OrderResponseMapper.ToResponse(
                order,
                itemsByOrder.GetValueOrDefault(order.Id, Array.Empty<OrderItem>()))).ToArray(),
            request.Specification.Page,
            request.Specification.PageSize,
            result.TotalCount);
    }
}

public sealed class UpdateOrderHandler(
    IOrderRepository orders,
    IOrderItemRepository orderItems,
    OrderFactory factory,
    DiscountStrategyResolver discounts,
    IUnitOfWork unitOfWork) : IRequestHandler<UpdateOrderCommand, OrderResponse?>
{
    public async Task<OrderResponse?> Handle(UpdateOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await orders.GetByIdAsync(request.Id, cancellationToken);
        if (order is null)
            return null;

        var drafts = request.Data.Items
            .Select(item => new OrderItemDraft(item.ProductName, item.Quantity, item.UnitPrice))
            .ToArray();
        var subtotal = drafts.Sum(item => item.Quantity * item.UnitPrice);
        var discountCode = string.IsNullOrWhiteSpace(request.Data.DiscountStrategy)
            ? null
            : request.Data.DiscountStrategy.Trim();
        var discountAmount = discountCode is null
            ? 0
            : discounts.Resolve(discountCode).CalculateDiscount(subtotal);

        var oldItems = await orderItems.ListByOrderIdAsync(order.Id, cancellationToken);
        var updatedItems = factory.Update(order, drafts, discountCode, discountAmount);
        orderItems.RemoveRange(oldItems);
        orderItems.AddRange(updatedItems);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return OrderResponseMapper.ToResponse(order, updatedItems);
    }
}

public sealed class GetOrderHandler(IOrderRepository orders, IOrderItemRepository orderItems)
    : IRequestHandler<GetOrderQuery, OrderResponse?>
{
    public async Task<OrderResponse?> Handle(GetOrderQuery request, CancellationToken cancellationToken)
    {
        var order = await orders.GetByIdAsync(request.Id, cancellationToken);
        if (order is null)
            return null;

        var items = await orderItems.ListByOrderIdAsync(order.Id, cancellationToken);
        return OrderResponseMapper.ToResponse(order, items);
    }
}

public sealed class DeleteOrderHandler(IOrderRepository orders, IUnitOfWork unitOfWork)
    : IRequestHandler<DeleteOrderCommand, bool>
{
    public async Task<bool> Handle(DeleteOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await orders.GetByIdAsync(request.Id, cancellationToken);
        if (order is null)
            return false;

        orders.Remove(order);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }
}
