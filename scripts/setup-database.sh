#!/bin/bash

# Setup script for PostgreSQL database
# Usage: ./setup-database.sh [dataset_size]

set -e

DATASET_SIZE=${1:-1000}
DB_HOST=${DB_HOST:-localhost}
DB_PORT=${DB_PORT:-5432}
DB_NAME=${DB_NAME:-benchmarkdb}
DB_USER=${DB_USER:-postgres}
DB_PASSWORD=${DB_PASSWORD:-postgres}

export PGPASSWORD=$DB_PASSWORD

echo "Setting up PostgreSQL database..."

# Create database
echo "Creating database if it doesn't exist..."
psql -h $DB_HOST -p $DB_PORT -U $DB_USER -d postgres -c "CREATE DATABASE $DB_NAME;" 2>/dev/null || echo "Database already exists"

# Initialize schema
echo "Creating schema..."
psql -h $DB_HOST -p $DB_PORT -U $DB_USER -d $DB_NAME -f "$(dirname "$0")/database-init.sql"

# Seed database
echo "Seeding database with $DATASET_SIZE products..."
psql -h $DB_HOST -p $DB_PORT -U $DB_USER -d $DB_NAME -c "
TRUNCATE TABLE products;

INSERT INTO products (id, name, description, price, created_at)
SELECT 
    gen_random_uuid(),
    'Product ' || generate_series,
    'Description for product ' || generate_series || '. This is a sample product description used for benchmarking purposes.',
    (RANDOM() * 1000 + 10)::NUMERIC(10, 2),
    CURRENT_TIMESTAMP - (RANDOM() * INTERVAL '365 days')
FROM generate_series(1, $DATASET_SIZE);

ANALYZE products;

SELECT COUNT(*) as total_products FROM products;
"

echo "Database setup complete!"

