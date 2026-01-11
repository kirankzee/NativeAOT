-- Seed script for PostgreSQL
-- Usage: psql -U postgres -d benchmarkdb -f seed-database.sql -v dataset_size=1000

\set dataset_size :dataset_size

-- Clear existing data if reseeding
TRUNCATE TABLE products;

-- Insert products
INSERT INTO products (id, name, description, price, created_at)
SELECT 
    gen_random_uuid(),
    'Product ' || generate_series,
    'Description for product ' || generate_series || '. This is a sample product description used for benchmarking purposes.',
    (RANDOM() * 1000 + 10)::NUMERIC(10, 2),
    CURRENT_TIMESTAMP - (RANDOM() * INTERVAL '365 days')
FROM generate_series(1, :dataset_size);

-- Analyze table for query optimizer
ANALYZE products;

-- Display count
SELECT COUNT(*) as total_products FROM products;

