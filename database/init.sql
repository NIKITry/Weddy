-- Инициализация базы данных Weddy
-- Этот скрипт создает полную схему базы данных

-- Создание enum для статуса приглашения
DO $$ BEGIN
    CREATE TYPE invitation_status AS ENUM ('none', 'yes', 'no', 'maybe');
EXCEPTION
    WHEN duplicate_object THEN null;
END $$;

-- Таблица настроек события (одна запись)
CREATE TABLE IF NOT EXISTS event_settings (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    invitation_text TEXT NOT NULL DEFAULT '',
    event_date TIMESTAMP,
    event_plan JSONB,
    wedding_couple_name VARCHAR(500),
    footer_note TEXT NOT NULL DEFAULT '',
    couple_display_name VARCHAR(500) NOT NULL DEFAULT '',
    created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP
);

-- Таблица приглашений
CREATE TABLE IF NOT EXISTS invitations (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    token VARCHAR(128) NOT NULL UNIQUE,
    display_name VARCHAR(500) NOT NULL,
    status invitation_status NOT NULL DEFAULT 'none',
    note TEXT NOT NULL DEFAULT '',
    invitation_text TEXT NOT NULL DEFAULT '',
    archived BOOLEAN NOT NULL DEFAULT false,
    meta_note VARCHAR(1000) NOT NULL DEFAULT '',
    created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP
);

-- Индексы для производительности
CREATE INDEX IF NOT EXISTS idx_invitations_token ON invitations(token) WHERE archived = false;
CREATE INDEX IF NOT EXISTS idx_invitations_status ON invitations(status);
CREATE INDEX IF NOT EXISTS idx_invitations_archived ON invitations(archived);
CREATE INDEX IF NOT EXISTS idx_invitations_display_name ON invitations(display_name);

-- Комментарии к таблицам
COMMENT ON TABLE event_settings IS 'Настройки события (общий текст приглашения, дата, план мероприятия)';
COMMENT ON TABLE invitations IS 'Приглашения для гостей';
COMMENT ON COLUMN invitations.token IS 'Уникальный токен для доступа к приглашению';
COMMENT ON COLUMN invitations.display_name IS 'Отображаемое имя гостя/группы';
COMMENT ON COLUMN invitations.status IS 'Статус ответа: none, yes, no, maybe';
COMMENT ON COLUMN invitations.archived IS 'Архивировано ли приглашение (скрыто от публичного доступа)';
COMMENT ON COLUMN invitations.invitation_text IS 'Индивидуальный текст приглашения для конкретного гостя/группы. Если пусто, используется общий текст из event_settings';
COMMENT ON COLUMN invitations.meta_note IS 'Мета-информация о приглашении для админа (например, "Родственники со стороны невесты"), видима только в админке';
COMMENT ON COLUMN event_settings.invitation_text IS 'Общий текст приглашения для всех гостей';
COMMENT ON COLUMN event_settings.event_date IS 'Дата мероприятия';
COMMENT ON COLUMN event_settings.event_plan IS 'План мероприятия в формате JSON: массив объектов с полями time, title, location';
COMMENT ON COLUMN event_settings.wedding_couple_name IS 'Текст "на чью свадьбу" (например, "Ивана и Марии"), общий для всех приглашений';
COMMENT ON COLUMN event_settings.footer_note IS 'Текст примечания, отображаемый внизу карточки приглашения после ответа RSVP';
COMMENT ON COLUMN event_settings.couple_display_name IS 'Имена пары для отображения на карточке приглашения (например, "Никита и Виолетта"), отображается как "Ваши [couple_display_name]"';

-- Создаем начальную запись в event_settings, если её нет
INSERT INTO event_settings (id)
SELECT gen_random_uuid()
WHERE NOT EXISTS (SELECT 1 FROM event_settings)
ON CONFLICT DO NOTHING;
