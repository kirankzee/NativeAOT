# PowerShell script to seed the database with test data
param(
    [Parameter(Mandatory=$false)]
    [ValidateSet(1000, 10000, 100000)]
    [int]$DatasetSize = 1000,
    
    [Parameter(Mandatory=$false)]
    [string]$Host = "localhost",
    
    [Parameter(Mandatory=$false)]
    [int]$Port = 5432,
    
    [Parameter(Mandatory=$false)]
    [string]$Database = "benchmarkdb",
    
    [Parameter(Mandatory=$false)]
    [string]$Username = "postgres",
    
    [Parameter(Mandatory=$false)]
    [string]$Password = "postgres"
)

$env:PGPASSWORD = $Password

Write-Host "Seeding database with $DatasetSize products..."

$connectionString = "host=$Host port=$Port dbname=$Database user=$Username"

# First, truncate the table
Write-Host "Clearing existing data..."
psql -c "TRUNCATE TABLE products;" $connectionString

# Generate and insert data using a SQL script
$tempScript = [System.IO.Path]::GetTempFileName() + ".sql"

@"
-- Insert products
INSERT INTO products (id, name, description, price, created_at)
SELECT 
    gen_random_uuid(),
    'Product ' || generate_series,
    'Description for product ' || generate_series || '. This is a sample product description used for benchmarking purposes.',
    (RANDOM() * 1000 + 10)::NUMERIC(10, 2),
    CURRENT_TIMESTAMP - (RANDOM() * INTERVAL '365 days')
FROM generate_series(1, $DatasetSize);

-- Analyze table
ANALYZE products;

-- Display count
SELECT COUNT(*) as total_products FROM products;
"@ | Out-File -FilePath $tempScript -Encoding UTF8

psql -f $tempScript $connectionString

Remove-Item $tempScript
Write-Host "Database seeded successfully!"

