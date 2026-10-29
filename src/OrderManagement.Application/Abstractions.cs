using OrderManagement.Domain;

namespace OrderManagement.Application;

public interface IRepository<T> where T : class
{
    Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    void Add(T entity);
    void Remove(T entity);
}

public interface IOrderRepository : IRepository<Order>
{
    Task<(IReadOnlyList<Order> Orders, int TotalCount)> ListAsync(
        OrderSpecification specification,
        CancellationToken cancellationToken);
}

public interface ICustomerRepository : IRepository<Customer>
{
    Task<Customer?> FindByEmailAsync(string email, CancellationToken cancellationToken);
}

public interface IOrderItemRepository : IRepository<OrderItem>
{
    Task<IReadOnlyList<OrderItem>> ListByOrderIdsAsync(
        IReadOnlyCollection<Guid> orderIds,
        CancellationToken cancellationToken);
    Task<IReadOnlyList<OrderItem>> ListByOrderIdAsync(Guid orderId, CancellationToken cancellationToken);
    void AddRange(IEnumerable<OrderItem> items);
    void RemoveRange(IEnumerable<OrderItem> items);
}

public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
