using Catalog.Api.Models;
using Catalog.Grpc;
using Grpc.Core;
using System.Globalization;
namespace Catalog.Api.Services;

public class ProductGrpcService
    (IProductRepository productRepository)
    : ProductProtoService.ProductProtoServiceBase
{
    /// <summary>
    /// Get product from grpc services
    /// </summary>
    /// <param name="request"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    /// <exception cref="RpcException"></exception>
    public override async Task<GetProductAvailabilityResponse> GetProductAvailability(
        GetProductAvailabilityRequest request,
        ServerCallContext context
    )
    {
        if (!Guid.TryParse(request.ProductId, out Guid productId))
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid product id format"));

        Product? product = await productRepository.Read(productId);

        if (product is null)
            return new GetProductAvailabilityResponse { Exists = false };

        return new GetProductAvailabilityResponse
        {
            Exists = true,
            AvailableStock = product.AvailableStock,
            Price = product.Price.ToString(CultureInfo.InvariantCulture)
        };
    }
}
