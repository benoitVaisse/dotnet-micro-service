
using Catalog.Api.Data;
using Catalog.Api.Models;
using Catalog.Api.Services;
using Microsoft.EntityFrameworkCore;

namespace Catalog.Api;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.

        builder.Services.AddControllers();

        builder.Services.AddDbContextPool<CatalogDbContext>(opt =>
            opt.UseNpgsql(builder.Configuration.GetConnectionString("CatalogDbContext")));

        builder.Services.AddScoped<IProductRepository, ProductRepository>();

        // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
        builder.Services.AddOpenApi();
        builder.Services.AddGrpc();
        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
        }

        app.UseHttpsRedirection();

        app.UseAuthorization();

        #region GRPC Service

        app.MapGrpcService<ProductGrpcService>();

        #endregion

        app.MapControllers();

        app.Run();
    }
}
