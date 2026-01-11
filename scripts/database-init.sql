-- Create database if it doesn't exist (run from postgres database)
-- Note: This script should be run from the postgres database, then connect to benchmarkdb
-- Or run: psql -U postgres -d postgres -c "CREATE DATABASE benchmarkdb;"
-- Then: psql -U postgres -d benchmarkdb -f database-init.sql

-- If running from within psql, uncomment:
-- \c benchmarkdb;

-- Create products table
CREATE TABLE IF NOT EXISTS products (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    name TEXT NOT NULL,
    description TEXT NOT NULL,
    price NUMERIC(10, 2) NOT NULL,
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
);

-- Create indexes for performance
CREATE INDEX IF NOT EXISTS idx_products_created_at ON products(created_at DESC);
CREATE INDEX IF NOT EXISTS idx_products_price ON products(price);

-- Grant permissions
GRANT ALL PRIVILEGES ON TABLE products TO postgres;
GRANT ALL PRIVILEGES ON ALL SEQUENCES IN SCHEMA public TO postgres;

