namespace Ordering.Domain.Models.Order;

public record Address
{
    public string Street { get; }
    public string City { get; }
    public string? State { get; }
    public string Country { get; }
    public string ZipCode { get; }

    private Address(string street, string city, string country, string zipCode, string? state)
    {
        Street = street;
        City = city;
        State = state;
        Country = country;
        ZipCode = zipCode;
    }

    public static Result<Address> Create(string street, string city, string country, string zipCode, string? state)
    {

        if (string.IsNullOrWhiteSpace(street))
            return Result<Address>.Failure("Street cannot be empty");
        if (string.IsNullOrWhiteSpace(city))
            return Result<Address>.Failure("City cannot be empty");
        if (string.IsNullOrWhiteSpace(country))
            return Result<Address>.Failure("Country cannot be empty");
        if (string.IsNullOrWhiteSpace(zipCode))
            return Result<Address>.Failure("ZipCode cannot be empty");

        return Result<Address>.Success(new Address(street, city, country, zipCode, state));
    }
}
