namespace OrderManagement.Domain;

public sealed class Customer
{
    private Customer() { }

    public Customer(Guid id, string name, string email)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainValidationException("Customer name is required.");
        if (string.IsNullOrWhiteSpace(email))
            throw new DomainValidationException("Customer email is required.");

        Id = id;
        Name = name.Trim();
        Email = email.Trim();
    }

    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
}
