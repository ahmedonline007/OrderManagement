using Microsoft.EntityFrameworkCore;
using OrderManagement.Application;
using OrderManagement.Domain;

namespace OrderManagement.Infrastructure;

public sealed class OrderRepository(OrderManagementDbContext dbContext) : IOrderRepository
{
    public async Task<Order?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        await dbContext.Orders
            .Include(order => order.Customer)
            .SingleOrDefaultAsync(order => order.Id == id, cancellationToken);

    public void Add(Order entity) => dbContext.Orders.Add(entity);

    public void Remove(Order entity) => dbContext.Orders.Remove(entity);

    public async Task<(IReadOnlyList<Order> Orders, int TotalCount)> ListAsync(
        OrderSpecification specification,
        CancellationToken cancellationToken)
    {
        IQueryable<Order> query = dbContext.Orders
            .AsNoTracking()
            .Include(order => order.Customer);

        if (specification.Criteria is not null)
            query = query.Where(specification.Criteria);

        var totalCount = await query.CountAsync(cancellationToken);
        IOrderedQueryable<Order> sorted = specification.SortBy switch
        {
            "customer" => specification.Descending
                ? query.OrderByDescending(order => order.Customer.Name)
                : query.OrderBy(order => order.Customer.Name),
            "total" => specification.Descending
                ? query.OrderByDescending(order => order.Subtotal - order.DiscountAmount)
                : query.OrderBy(order => order.Subtotal - order.DiscountAmount),
            _ => specification.Descending
                ? query.OrderByDescending(order => order.CreatedAtUtc)
                : query.OrderBy(order => order.CreatedAtUtc)
        };

        var page = await sorted
            .Skip((specification.Page - 1) * specification.PageSize)
            .Take(specification.PageSize)
            .ToListAsync(cancellationToken);
        return (page, totalCount);
    }
}

public sealed class OrderItemRepository(OrderManagementDbContext dbContext) : IOrderItemRepository
{
    public Task<OrderItem?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        dbContext.OrderItems.SingleOrDefaultAsync(item => item.Id == id, cancellationToken);

    public async Task<IReadOnlyList<OrderItem>> ListByOrderIdsAsync(
        IReadOnlyCollection<Guid> orderIds,
        CancellationToken cancellationToken)
    {
        if (orderIds.Count == 0)
            return [];

        return await dbContext.OrderItems
            .AsNoTracking()
            .Where(item => orderIds.Contains(item.OrderId))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<OrderItem>> ListByOrderIdAsync(
        Guid orderId,
        CancellationToken cancellationToken) =>
        await dbContext.OrderItems
            .Where(item => item.OrderId == orderId)
            .ToListAsync(cancellationToken);

    public void Add(OrderItem entity) => dbContext.OrderItems.Add(entity);
    public void Remove(OrderItem entity) => dbContext.OrderItems.Remove(entity);
    public void AddRange(IEnumerable<OrderItem> items) => dbContext.OrderItems.AddRange(items);
    public void RemoveRange(IEnumerable<OrderItem> items) => dbContext.OrderItems.RemoveRange(items);
}

public sealed class CustomerRepository(OrderManagementDbContext dbContext) : ICustomerRepository
{
    public Task<Customer?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        dbContext.Customers.SingleOrDefaultAsync(customer => customer.Id == id, cancellationToken);

    public Task<Customer?> FindByEmailAsync(string email, CancellationToken cancellationToken) =>
        dbContext.Customers.SingleOrDefaultAsync(
            customer => customer.Email == email,
            cancellationToken);

    public void Add(Customer entity) => dbContext.Customers.Add(entity);

    public void Remove(Customer entity) => dbContext.Customers.Remove(entity);
}

public sealed class UnitOfWork(OrderManagementDbContext dbContext) : IUnitOfWork
{
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken) =>
        dbContext.SaveChangesAsync(cancellationToken);
}
