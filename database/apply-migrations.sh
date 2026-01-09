#!/bin/bash
# Скрипт для применения миграций базы данных
# Использует переменные окружения из docker-compose

set -e

PGHOST="${POSTGRES_HOST:-postgres}"
PGPORT="${POSTGRES_PORT:-5432}"
PGDATABASE="${POSTGRES_DB:-weddy}"
PGUSER="${POSTGRES_USER:-postgres}"
PGPASSWORD="${POSTGRES_PASSWORD:-postgres}"

export PGHOST PGPORT PGDATABASE PGUSER PGPASSWORD

echo "Проверка подключения к базе данных..."
until PGPASSWORD="$PGPASSWORD" psql -h "$PGHOST" -U "$PGUSER" -d "$PGDATABASE" -c '\q' 2>/dev/null; do
  echo "Ожидание PostgreSQL..."
  sleep 1
done

echo "Проверка существования таблиц..."
TABLE_EXISTS=$(PGPASSWORD="$PGPASSWORD" psql -h "$PGHOST" -U "$PGUSER" -d "$PGDATABASE" -tAc "SELECT EXISTS (SELECT FROM information_schema.tables WHERE table_schema = 'public' AND table_name = 'event_settings');" 2>/dev/null || echo "false")

if [ "$TABLE_EXISTS" = "t" ]; then
  echo "Таблицы уже существуют. Миграции применены."
else
  echo "Применение миграций..."
  PGPASSWORD="$PGPASSWORD" psql -h "$PGHOST" -U "$PGUSER" -d "$PGDATABASE" -f /app/database/init.sql
  echo "Миграции применены успешно."
fi

