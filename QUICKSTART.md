# Быстрый старт

## Запуск

```bash
# 1. Создайте .env файл (скопируйте и заполните)
cp .env.example .env

# 2. Запустите все сервисы
docker-compose up -d --build

# 3. Проверьте статус
docker-compose ps
```

## Переменные окружения (.env)

### Обязательные (измените перед production!)

```bash
# База данных
POSTGRES_PASSWORD=your-secure-password        # Пароль для PostgreSQL
ADMIN_API_KEY=your-secret-key                 # Ключ для доступа к админке

# Домен (замените на ваш)
BASE_URL=https://weddy.example.com            # Ваш домен
CORS_ORIGINS=https://weddy.example.com        # Разрешенные домены для CORS
INVITATION_BASE_URL=https://weddy.example.com # URL для ссылок приглашений
```

### Опциональные

```bash
# База данных
POSTGRES_DB=weddy                             # Имя БД (по умолчанию: weddy)
POSTGRES_USER=postgres                        # Пользователь БД (по умолчанию: postgres)

# API
API_BASE_URL=/api                             # URL для API (по умолчанию: /api)

# Nginx порты
NGINX_PORT=80                                 # HTTP порт (по умолчанию: 80)
NGINX_HTTPS_PORT=443                          # HTTPS порт (по умолчанию: 443)
```

## Проверка

```bash
# Health check
curl http://localhost/health

# Логи
docker-compose logs -f

# Остановка
docker-compose down

# Остановка с удалением данных БД
docker-compose down -v
```

## Миграции

Применяются автоматически при первом запуске PostgreSQL.

