# Установка Docker и Docker Compose на сервер

## Ubuntu/Debian

```bash
# 1. Обновление системы
sudo apt update
sudo apt upgrade -y

# 2. Установка зависимостей
sudo apt install -y ca-certificates curl gnupg lsb-release

# 3. Добавление официального GPG ключа Docker
sudo mkdir -p /etc/apt/keyrings
curl -fsSL https://download.docker.com/linux/ubuntu/gpg | sudo gpg --dearmor -o /etc/apt/keyrings/docker.gpg

# 4. Добавление репозитория Docker
echo \
  "deb [arch=$(dpkg --print-architecture) signed-by=/etc/apt/keyrings/docker.gpg] https://download.docker.com/linux/ubuntu \
  $(lsb_release -cs) stable" | sudo tee /etc/apt/sources.list.d/docker.list > /dev/null

# 5. Установка Docker
sudo apt update
sudo apt install -y docker-ce docker-ce-cli containerd.io docker-buildx-plugin docker-compose-plugin

# 6. Добавление пользователя в группу docker (чтобы не использовать sudo)
sudo usermod -aG docker $USER
newgrp docker

# 7. Проверка установки
docker --version
docker compose version
```

## CentOS/RHEL

```bash
# 1. Установка зависимостей
sudo yum install -y yum-utils

# 2. Добавление репозитория Docker
sudo yum-config-manager --add-repo https://download.docker.com/linux/centos/docker-ce.repo

# 3. Установка Docker
sudo yum install -y docker-ce docker-ce-cli containerd.io docker-buildx-plugin docker-compose-plugin

# 4. Запуск Docker
sudo systemctl start docker
sudo systemctl enable docker

# 5. Добавление пользователя в группу docker
sudo usermod -aG docker $USER
newgrp docker

# 6. Проверка
docker --version
docker compose version
```

## Клонирование проекта

```bash
# 1. Установите git (если нет)
sudo apt install -y git  # Ubuntu/Debian
# или
sudo yum install -y git  # CentOS/RHEL

# 2. Клонируйте репозиторий
git clone https://github.com/NIKITry/Weddy.git
cd Weddy

# 3. Создайте .env файл
cp .env.example .env
nano .env  # отредактируйте под свои нужды

# 4. Запустите
docker compose up -d --build
```

## Проверка

```bash
# Статус контейнеров
docker compose ps

# Логи
docker compose logs -f

# Health check
curl http://localhost/health
```

