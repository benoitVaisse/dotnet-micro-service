using Ordering.Domain.Shared;

namespace Ordering.Domain.Models.Client;

public class Buyer : AggregateEntity
{
    public string Name { get; private set; }

    private Buyer(string name, Guid id)
    {
        Name = name;
        Id = id;
    }

    public static Result<Buyer> Create(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Result<Buyer>.Failure("Name can not be null");

        return Result<Buyer>.Success(new Buyer(name, Guid.NewGuid()));
    }
}
