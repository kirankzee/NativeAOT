using Npgsql;
using Shared.Models;

namespace Shared.DataAccess;

public sealed class ProductRepository : IDisposable
{
    private readonly NpgsqlDataSource _dataSource;
    private readonly ILogger<ProductRepository> _logger;

    public ProductRepository(string connectionString, ILogger<ProductRepository> logger)
    {
        _logger = logger;
        var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);
        _dataSource = dataSourceBuilder.Build();
    }

    public async Task<Product?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        const string sql = """
            SELECT id, name, description, price, created_at
            FROM products
            WHERE id = $1
            """;

        await using var command = _dataSource.CreateCommand(sql);
        command.Parameters.AddWithValue(id);

        await using var reader = await command.ExecuteReaderAsync(ct);
        if (await reader.ReadAsync(ct))
        {
            return new Product
            {
                Id = reader.GetGuid(0),
                Name = reader.GetString(1),
                Description = reader.GetString(2),
                Price = reader.GetDecimal(3),
                CreatedAt = reader.GetDateTime(4)
            };
        }

        return null;
    }

    public async Task<PaginatedResponse<Product>> GetPaginatedAsync(int page, int pageSize, CancellationToken ct = default)
    {
        const string countSql = "SELECT COUNT(*) FROM products";
        const string dataSql = """
            SELECT id, name, description, price, created_at
            FROM products
            ORDER BY created_at DESC
            LIMIT $1 OFFSET $2
            """;

        // Get total count
        await using var countCommand = _dataSource.CreateCommand(countSql);
        var totalCount = Convert.ToInt32(await countCommand.ExecuteScalarAsync(ct) ?? 0);

        // Get paginated data
        var offset = (page - 1) * pageSize;
        await using var dataCommand = _dataSource.CreateCommand(dataSql);
        dataCommand.Parameters.AddWithValue(pageSize);
        dataCommand.Parameters.AddWithValue(offset);

        var products = new List<Product>();
        await using var reader = await dataCommand.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
        {
            products.Add(new Product
            {
                Id = reader.GetGuid(0),
                Name = reader.GetString(1),
                Description = reader.GetString(2),
                Price = reader.GetDecimal(3),
                CreatedAt = reader.GetDateTime(4)
            });
        }

        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        return new PaginatedResponse<Product>
        {
            Items = products,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = totalPages
        };
    }

    public async Task<Product> CreateAsync(CreateProductRequest request, CancellationToken ct = default)
    {
        const string sql = """
            INSERT INTO products (id, name, description, price, created_at)
            VALUES ($1, $2, $3, $4, $5)
            RETURNING id, name, description, price, created_at
            """;

        var id = Guid.NewGuid();
        var createdAt = DateTime.UtcNow;

        await using var command = _dataSource.CreateCommand(sql);
        command.Parameters.AddWithValue(id);
        command.Parameters.AddWithValue(request.Name);
        command.Parameters.AddWithValue(request.Description);
        command.Parameters.AddWithValue(request.Price);
        command.Parameters.AddWithValue(createdAt);

        await using var reader = await command.ExecuteReaderAsync(ct);
        if (await reader.ReadAsync(ct))
        {
            return new Product
            {
                Id = reader.GetGuid(0),
                Name = reader.GetString(1),
                Description = reader.GetString(2),
                Price = reader.GetDecimal(3),
                CreatedAt = reader.GetDateTime(4)
            };
        }

        throw new InvalidOperationException("Failed to create product");
    }

    public async Task<Product?> UpdateAsync(Guid id, UpdateProductRequest request, CancellationToken ct = default)
    {
        const string sql = """
            UPDATE products
            SET name = $1, description = $2, price = $3
            WHERE id = $4
            RETURNING id, name, description, price, created_at
            """;

        await using var command = _dataSource.CreateCommand(sql);
        command.Parameters.AddWithValue(request.Name);
        command.Parameters.AddWithValue(request.Description);
        command.Parameters.AddWithValue(request.Price);
        command.Parameters.AddWithValue(id);

        await using var reader = await command.ExecuteReaderAsync(ct);
        if (await reader.ReadAsync(ct))
        {
            return new Product
            {
                Id = reader.GetGuid(0),
                Name = reader.GetString(1),
                Description = reader.GetString(2),
                Price = reader.GetDecimal(3),
                CreatedAt = reader.GetDateTime(4)
            };
        }

        return null;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        const string sql = "DELETE FROM products WHERE id = $1";

        await using var command = _dataSource.CreateCommand(sql);
        command.Parameters.AddWithValue(id);

        return await command.ExecuteNonQueryAsync(ct) > 0;
    }

    public async Task<IReadOnlyList<Product>> GetBulkAsync(int limit, CancellationToken ct = default)
    {
        const string sql = """
            SELECT id, name, description, price, created_at
            FROM products
            ORDER BY created_at DESC
            LIMIT $1
            """;

        var products = new List<Product>();
        await using var command = _dataSource.CreateCommand(sql);
        command.Parameters.AddWithValue(limit);

        await using var reader = await command.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
        {
            products.Add(new Product
            {
                Id = reader.GetGuid(0),
                Name = reader.GetString(1),
                Description = reader.GetString(2),
                Price = reader.GetDecimal(3),
                CreatedAt = reader.GetDateTime(4)
            });
        }

        return products;
    }

    public void Dispose()
    {
        _dataSource?.Dispose();
    }
}

