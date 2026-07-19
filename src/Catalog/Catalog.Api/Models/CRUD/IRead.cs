namespace Catalog.Api.Models.CRUD;

public interface IRead<Entity> where Entity : IEntity
{
    public Task<Entity?> Read(Guid id);
}
