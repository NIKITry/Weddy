# Weddy RSVP

Система управления свадебными приглашениями с RSVP функционалом.

## Описание

Приложение для создания и управления свадебными приглашениями:
- **Админ-панель** - создание приглашений, настройка события, просмотр ответов
- **Страницы приглашений** - персонализированные карточки для гостей с возможностью ответа (RSVP)
- **API** - REST API для управления данными

## Технологии

- **Backend:** ASP.NET Core (Minimal APIs), PostgreSQL, Dapper
- **Frontend:** Alpine.js, Tailwind CSS
- **Инфраструктура:** Docker, Docker Compose, Nginx

## Быстрый старт

```bash
# 1. Создайте .env файл с переменными окружения
POSTGRES_PASSWORD=your-password
ADMIN_API_KEY=your-secret-key
BASE_URL=https://weddy.example.com
CORS_ORIGINS=https://weddy.example.com
INVITATION_BASE_URL=https://weddy.example.com

# 2. Запустите
docker compose up -d --build

# 3. Проверьте
curl http://localhost/health
```

Подробная инструкция в [QUICKSTART.md](QUICKSTART.md)

## Структура

- `/admin` - админ-панель
- `/i/{token}` - страница приглашения
- `/api/*` - API endpoints

## Деплой

Приложение готово к деплою на сервер с Docker. Все миграции применяются автоматически при первом запуске.

