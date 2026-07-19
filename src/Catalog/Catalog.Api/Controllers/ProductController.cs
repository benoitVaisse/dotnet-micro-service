using Catalog.Api.Models;
using Catalog.Api.Models.Filter;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Catalog.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ProductController : ControllerBase
{
    [HttpPost("filter")]
    [ProducesResponseType<IEnumerable<Product>>(StatusCodes.Status200OK)]
    public async Task<Ok<IEnumerable<Product>>> GetFilterdProduct(
            [FromBody] FilterRequest request,
            [FromServices] IProductRepository productRepository
        )
    {
        return TypedResults.Ok(await productRepository.GetFiltered(request));
    }

    [HttpPost]
    [ProducesResponseType<Product>(StatusCodes.Status200OK)]
    public async Task<CreatedAtRoute<Product>> Create(
            [FromBody] Product request,
            [FromServices] IProductRepository productRepository
        )
    {
        Product productCreated = await productRepository.Add(request);
        await productRepository.SaveChangeAsync();
        return TypedResults.CreatedAtRoute(productCreated, nameof(Read), new { id = productCreated.Id });
    }

    [HttpGet("{id:guid}", Name = nameof(Read))]
    [ProducesResponseType<Product>(StatusCodes.Status200OK)]
    [ProducesResponseType<Product>(StatusCodes.Status404NotFound)]
    public async Task<Results<Ok<Product>, NotFound>> Read(
            Guid id,
            [FromServices] IProductRepository productRepository
        )
    {
        Product? product = await productRepository.Read(id);
        if (product == null)
            return TypedResults.NotFound();

        return TypedResults.Ok(product);
    }

    [HttpPatch("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<Results<NoContent, BadRequest<string>>> Update(
            Guid id,
            [FromBody] Product request,
            [FromServices] IProductRepository productRepository
        )
    {
        Product? product = await productRepository.ReadTracked(id);
        if (product == null)
            return TypedResults.BadRequest("Product don't exist");

        product.AvailableStock = request.AvailableStock;
        product.Description = request.Description;
        product.Name = request.Name;
        product.Price = request.Price;


        await productRepository.SaveChangeAsync();

        return TypedResults.NoContent();
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<Results<NoContent, BadRequest<string>>> Delete(
            Guid id,
            [FromServices] IProductRepository productRepository
        )
    {
        Product? product = await productRepository.ReadTracked(id);
        if (product == null)
            return TypedResults.BadRequest("Product don't exist");

        productRepository.Delete(product);
        await productRepository.SaveChangeAsync();

        return TypedResults.NoContent();
    }
}
