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
}
