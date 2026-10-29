namespace OrderManagement.Application;

public interface IDiscountStrategy
{
    string Code { get; }
    string Name { get; }
    decimal CalculateDiscount(decimal subtotal);
}

public sealed class PercentageDiscountStrategy : IDiscountStrategy
{
    public string Code => "percentage";
    public string Name => "10% off";
    public decimal CalculateDiscount(decimal subtotal) => decimal.Round(subtotal * 0.10m, 2);
}

public sealed class FixedDiscountStrategy : IDiscountStrategy
{
    public string Code => "fixed";
    public string Name => "$10 off";
    public decimal CalculateDiscount(decimal subtotal) => Math.Min(10m, subtotal);
}

public sealed class DiscountStrategyResolver(IEnumerable<IDiscountStrategy> strategies)
{
    public IReadOnlyCollection<IDiscountStrategy> All => strategies.ToArray();

    public IDiscountStrategy Resolve(string code) =>
        strategies.FirstOrDefault(strategy => string.Equals(strategy.Code, code, StringComparison.OrdinalIgnoreCase))
        ?? throw new ArgumentException($"Unknown discount strategy '{code}'.", nameof(code));
}
