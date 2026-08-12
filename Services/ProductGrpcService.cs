using Grpc.Core;
using Microsoft.EntityFrameworkCore;
using ProductGrpc.Protos;
using ProductGrpc.Server.Data;

namespace ProductGrpc.Server.Services;

/// <summary>
/// Implements the ProductService contract from Protos/product.proto.
/// The base class <c>ProductServiceBase</c> is generated at build time by Grpc.Tools.
/// </summary>
public class ProductGrpcService(ProductDbContext db, ILogger<ProductGrpcService> logger)
    : ProductService.ProductServiceBase
{
    public override async Task<ProductModel> GetProduct(GetProductRequest request, ServerCallContext context)
    {
        var product = await db.Products.FindAsync([request.Id], context.CancellationToken)
            ?? throw new RpcException(new Status(StatusCode.NotFound, $"Product {request.Id} was not found."));

        return ToModel(product);
    }

    public override async Task<ListProductsResponse> ListProducts(ListProductsRequest request, ServerCallContext context)
    {
        var query = db.Products.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(request.NameFilter))
        {
            query = query.Where(p => EF.Functions.Like(p.Name, $"%{request.NameFilter}%"));
        }

        var products = await query.OrderBy(p => p.Id).ToListAsync(context.CancellationToken);

        var response = new ListProductsResponse();
        response.Products.AddRange(products.Select(ToModel));
        return response;
    }

    public override async Task<ProductModel> CreateProduct(CreateProductRequest request, ServerCallContext context)
    {
        Validate(request.Name, request.Price, request.Stock);

        var product = new Product
        {
            Name = request.Name,
            Description = request.Description,
            Price = (decimal)request.Price,
            Stock = request.Stock
        };

        db.Products.Add(product);
        await db.SaveChangesAsync(context.CancellationToken);

        logger.LogInformation("Created product {ProductId}", product.Id);
        return ToModel(product);
    }

    public override async Task<ProductModel> UpdateProduct(UpdateProductRequest request, ServerCallContext context)
    {
        Validate(request.Name, request.Price, request.Stock);

        var product = await db.Products.FindAsync([request.Id], context.CancellationToken)
            ?? throw new RpcException(new Status(StatusCode.NotFound, $"Product {request.Id} was not found."));

        product.Name = request.Name;
        product.Description = request.Description;
        product.Price = (decimal)request.Price;
        product.Stock = request.Stock;

        await db.SaveChangesAsync(context.CancellationToken);

        logger.LogInformation("Updated product {ProductId}", product.Id);
        return ToModel(product);
    }

    public override async Task<DeleteProductResponse> DeleteProduct(DeleteProductRequest request, ServerCallContext context)
    {
        var product = await db.Products.FindAsync([request.Id], context.CancellationToken)
            ?? throw new RpcException(new Status(StatusCode.NotFound, $"Product {request.Id} was not found."));

        db.Products.Remove(product);
        await db.SaveChangesAsync(context.CancellationToken);

        logger.LogInformation("Deleted product {ProductId}", request.Id);
        return new DeleteProductResponse { Success = true };
    }

    private static void Validate(string name, double price, int stock)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Name is required."));
        }

        if (price < 0)
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Price cannot be negative."));
        }

        if (stock < 0)
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Stock cannot be negative."));
        }
    }

    private static ProductModel ToModel(Product product) => new()
    {
        Id = product.Id,
        Name = product.Name,
        Description = product.Description,
        Price = (double)product.Price,
        Stock = product.Stock
    };
}
