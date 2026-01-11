using System.Text.Json;
using System.Text.Json.Serialization;
using Shared.DataAccess;
using Shared.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.TypeInfoResolver = SourceGenerationContext.Default;
});

builder.Services.AddSingleton<ProductRepository>(sp =>
{
    var connectionString = builder.Configuration.GetConnectionString("PostgreSQL")
        ?? throw new InvalidOperationException("PostgreSQL connection string is required");
    var logger = sp.GetRequiredService<ILogger<ProductRepository>>();
    return new ProductRepository(connectionString, logger);
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapPost("/products", async (
    CreateProductRequest request,
    ProductRepository repository,
    CancellationToken ct) =>
{
    var product = await repository.CreateAsync(request, ct);
    return Results.Created($"/products/{product.Id}", product);
})
.WithName("CreateProduct")
.WithOpenApi();

app.MapGet("/products/{id:guid}", async (
    Guid id,
    ProductRepository repository,
    CancellationToken ct) =>
{
    var product = await repository.GetByIdAsync(id, ct);
    return product is not null ? Results.Ok(product) : Results.NotFound();
})
.WithName("GetProduct")
.WithOpenApi();

app.MapGet("/products", async (
    int page = 1,
    int pageSize = 20,
    ProductRepository? repository = null,
    CancellationToken ct = default) =>
{
    if (repository is null) return Results.Problem("Repository not available");
    if (page < 1) page = 1;
    if (pageSize < 1 || pageSize > 100) pageSize = 20;

    var result = await repository.GetPaginatedAsync(page, pageSize, ct);
    return Results.Ok(result);
})
.WithName("GetProducts")
.WithOpenApi();

app.MapPut("/products/{id:guid}", async (
    Guid id,
    UpdateProductRequest request,
    ProductRepository repository,
    CancellationToken ct) =>
{
    var product = await repository.UpdateAsync(id, request, ct);
    return product is not null ? Results.Ok(product) : Results.NotFound();
})
.WithName("UpdateProduct")
.WithOpenApi();

app.MapDelete("/products/{id:guid}", async (
    Guid id,
    ProductRepository repository,
    CancellationToken ct) =>
{
    var deleted = await repository.DeleteAsync(id, ct);
    return deleted ? Results.NoContent() : Results.NotFound();
})
.WithName("DeleteProduct")
.WithOpenApi();

app.MapGet("/products/bulk", async (
    int limit = 1000,
    ProductRepository? repository = null,
    CancellationToken ct = default) =>
{
    if (repository is null) return Results.Problem("Repository not available");
    if (limit < 1 || limit > 10000) limit = 1000;

    var products = await repository.GetBulkAsync(limit, ct);
    return Results.Ok(products);
})
.WithName("GetBulkProducts")
.WithOpenApi();

app.MapGet("/health", () => Results.Ok(new { status = "healthy", timestamp = DateTime.UtcNow }))
    .WithName("Health")
    .WithOpenApi();

app.Run();

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
[JsonSerializable(typeof(Product))]
[JsonSerializable(typeof(CreateProductRequest))]
[JsonSerializable(typeof(UpdateProductRequest))]
[JsonSerializable(typeof(PaginatedResponse<Product>))]
[JsonSerializable(typeof(IReadOnlyList<Product>))]
internal partial class SourceGenerationContext : JsonSerializerContext { }

