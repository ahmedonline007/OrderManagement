using System.Linq.Expressions;
using OrderManagement.Domain;

namespace OrderManagement.Application;

public sealed class OrderSpecification
{
    public OrderSpecification(string? search, string? sortBy, bool descending, int page, int pageSize)
    {
        Search = search?.Trim();
        SortBy = sortBy?.Trim().ToLowerInvariant() switch
        {
            "customer" => "customer",
            "total" => "total",
            _ => "date"
        };
        Descending = descending;
        Page = Math.Max(page, 1);
        PageSize = Math.Clamp(pageSize, 1, 100);
    }

    public string? Search { get; }
    public string SortBy { get; }
    public bool Descending { get; }
    public int Page { get; }
    public int PageSize { get; }

    public Expression<Func<Order, bool>>? Criteria => string.IsNullOrWhiteSpace(Search)
        ? null
        : order => order.Customer.Name.Contains(Search) || order.Customer.Email.Contains(Search);
}
