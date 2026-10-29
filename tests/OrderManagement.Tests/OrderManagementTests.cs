using OrderManagement.Application;
using OrderManagement.Domain;

namespace OrderManagement.Tests;

public sealed class OrderManagementTests
{
    [Fact]
    public void OrderFactoryCreatesOrderWithItemsAndDiscount()
    {
        var customer = new Customer(Guid.NewGuid(), "Ada Lovelace", "ada@example.com");
        var factory = new OrderFactory();

        var creation = factory.Create(
            customer,
            [
                new OrderItemDraft("Notebook", 2, 10m),
                new OrderItemDraft("Pen", 1, 5m)
            ],
            "percentage",
            2.50m);

        Assert.Equal(customer.Id, creation.Order.CustomerId);
        Assert.Equal(2, creation.Items.Count);
        Assert.Equal(25m, creation.Order.Subtotal);
        Assert.Equal(2.50m, creation.Order.DiscountAmount);
        Assert.Equal(22.50m, creation.Order.Total);
    }

    [Fact]
    public void OrderFactoryRejectsOrdersWithoutItems()
    {
        var customer = new Customer(Guid.NewGuid(), "Ada Lovelace", "ada@example.com");
        var factory = new OrderFactory();

        Assert.Throws<DomainValidationException>(() =>
            factory.Create(customer, Array.Empty<OrderItemDraft>()));
    }

    [Fact]
    public void OrderFactoryRejectsDiscountGreaterThanSubtotal()
    {
        var customer = new Customer(Guid.NewGuid(), "Ada Lovelace", "ada@example.com");
        var factory = new OrderFactory();

        Assert.Throws<DomainValidationException>(() =>
            factory.Create(customer, [new OrderItemDraft("Notebook", 1, 10m)], "fixed", 11m));
    }

    [Fact]
    public void OrderFactoryUpdatesOrderAndReturnsIndependentItemRoots()
    {
        var customer = new Customer(Guid.NewGuid(), "Ada Lovelace", "ada@example.com");
        var factory = new OrderFactory();
        var creation = factory.Create(customer, [new OrderItemDraft("Notebook", 1, 10m)]);

        var updatedItems = factory.Update(
            creation.Order,
            [new OrderItemDraft("Notebook", 2, 10m)],
            "fixed",
            10m);

        Assert.Single(updatedItems);
        Assert.Equal(creation.Order.Id, updatedItems[0].OrderId);
        Assert.Equal(20m, creation.Order.Subtotal);
        Assert.Equal(10m, creation.Order.Total);
    }

    [Fact]
    public void DiscountStrategiesCalculateAndResolveDiscounts()
    {
        var percentage = new PercentageDiscountStrategy();
        var fixedDiscount = new FixedDiscountStrategy();
        var resolver = new DiscountStrategyResolver([percentage, fixedDiscount]);

        Assert.Equal(12.50m, resolver.Resolve("PERCENTAGE").CalculateDiscount(125m));
        Assert.Equal(10m, resolver.Resolve("fixed").CalculateDiscount(125m));
        Assert.Equal(3m, fixedDiscount.CalculateDiscount(3m));
        Assert.Throws<ArgumentException>(() => resolver.Resolve("unknown"));
    }

    [Fact]
    public void OrderSpecificationClampsPagingAndWhitelistsSort()
    {
        var specification = new OrderSpecification(null, "untrusted", descending: false, page: 0, pageSize: 500);

        Assert.Equal(1, specification.Page);
        Assert.Equal(100, specification.PageSize);
        Assert.Equal("date", specification.SortBy);
        Assert.Null(specification.Criteria);
    }
}
