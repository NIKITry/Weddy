var builder = WebApplication.CreateBuilder(args);

// Configuration
var apiBaseUrl = builder.Configuration["ApiBaseUrl"] ?? "http://localhost:5000";
var invitationBaseUrl = builder.Configuration["InvitationBaseUrl"] ?? "http://localhost:5002";
var adminApiKey = builder.Configuration["Admin:ApiKey"] 
    ?? throw new InvalidOperationException("Admin:ApiKey not found in configuration.");

// Нормализуем InvitationBaseUrl - добавляем протокол если отсутствует
// Предполагается, что URL в конфигурации корректен, поэтому только добавляем протокол при необходимости
if (!string.IsNullOrWhiteSpace(invitationBaseUrl) && 
    !invitationBaseUrl.StartsWith("http://", StringComparison.OrdinalIgnoreCase) && 
    !invitationBaseUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
{
    // Если нет протокола, добавляем https:// (или http:// для localhost)
    if (invitationBaseUrl.Contains("localhost", StringComparison.OrdinalIgnoreCase) || 
        invitationBaseUrl.Contains("127.0.0.1", StringComparison.OrdinalIgnoreCase))
    {
        invitationBaseUrl = $"http://{invitationBaseUrl}";
    }
    else
    {
        invitationBaseUrl = $"https://{invitationBaseUrl}";
    }
}

// Настраиваем WebRootPath если не установлен
if (string.IsNullOrEmpty(builder.Environment.WebRootPath))
{
    var appDirectory = AppContext.BaseDirectory;
    var wwwrootPath = Path.Combine(appDirectory, "wwwroot");
    if (Directory.Exists(wwwrootPath))
    {
        builder.Environment.WebRootPath = wwwrootPath;
    }
    else
    {
        // Пробуем путь относительно текущей директории
        var currentDirWwwroot = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
        if (Directory.Exists(currentDirWwwroot))
        {
            builder.Environment.WebRootPath = currentDirWwwroot;
        }
        else
        {
            // Последняя попытка - используем appDirectory как базовый путь
            builder.Environment.WebRootPath = appDirectory;
        }
    }
}

var app = builder.Build();

// Вспомогательная функция для получения HTML формы логина
static string GetLoginHtml()
{
    return @"
<!DOCTYPE html>
<html lang=""ru"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>Weddy RSVP - Вход в админ-панель</title>
    <script src=""https://cdn.tailwindcss.com""></script>
    <script defer src=""https://cdn.jsdelivr.net/npm/alpinejs@3.x.x/dist/cdn.min.js""></script>
</head>
<body class=""bg-gradient-to-br from-pink-50 to-purple-50 min-h-screen flex items-center justify-center px-2 sm:px-4"">
    <div class=""bg-white rounded-lg shadow-lg p-4 sm:p-8 max-w-md w-full"" x-data=""loginApp()"">
        <h1 class=""text-2xl sm:text-3xl font-bold text-center mb-4 sm:mb-6 text-purple-600"">🔐 Админ-панель</h1>
        <div class=""space-y-4"">
            <div>
                <label class=""block text-sm font-medium text-gray-700 mb-2"">API ключ</label>
                <input 
                    x-model=""adminKeyInput""
                    @keyup.enter=""login()""
                    type=""password"" 
                    placeholder=""Введите API ключ""
                    class=""w-full p-3 border rounded-lg focus:ring-2 focus:ring-pink-500 text-sm sm:text-base"">
            </div>
            <div class=""flex items-center"">
                <input 
                    x-model=""rememberMe""
                    type=""checkbox"" 
                    id=""rememberMe""
                    class=""h-4 w-4 text-purple-600 focus:ring-purple-500 border-gray-300 rounded"">
                <label for=""rememberMe"" class=""ml-2 block text-xs sm:text-sm text-gray-700"">
                    Запомнить меня
                </label>
            </div>
            <button 
                @click=""login()""
                class=""w-full bg-purple-500 hover:bg-purple-600 text-white px-4 sm:px-6 py-2 sm:py-3 rounded-lg font-semibold text-sm sm:text-base"">
                Войти
            </button>
            <div x-show=""errorMessage"" class=""text-red-600 text-xs sm:text-sm text-center"" x-text=""errorMessage""></div>
        </div>
    </div>
    <script>
        function loginApp() {
            return {
                adminKeyInput: '',
                rememberMe: false,
                errorMessage: '',
                getLoginPath() {
                    // Определяем правильный путь для логина на основе текущего URL
                    const currentPath = window.location.pathname;
                    if (currentPath.startsWith('/admin')) {
                        return '/admin/login';
                    }
                    return '/login';
                },
                async login() {
                    if (!this.adminKeyInput.trim()) {
                        this.errorMessage = 'Введите API ключ';
                        return;
                    }
                    this.errorMessage = '';
                    try {
                        const formData = new FormData();
                        formData.append('key', this.adminKeyInput.trim());
                        formData.append('rememberMe', this.rememberMe);
                        
                        const loginPath = this.getLoginPath();
                        const response = await fetch(loginPath, {
                            method: 'POST',
                            body: formData,
                            credentials: 'same-origin'
                        });
                        
                        if (response.status === 200) {
                            // Получаем путь для редиректа из ответа
                            const data = await response.json();
                            const redirectPath = data.redirect || (window.location.pathname.startsWith('/admin') ? '/admin/' : '/');
                            setTimeout(() => {
                                window.location.href = redirectPath;
                            }, 200);
                        } else {
                            const errorText = await response.text();
                            this.errorMessage = errorText || 'Неверный API ключ';
                        }
                    } catch (error) {
                        this.errorMessage = 'Ошибка подключения к серверу';
                    }
                }
            };
        }
    </script>
</body>
</html>";
}

// Login endpoint - POST запрос для проверки ключа
app.MapPost("/login", async (HttpContext context) =>
{
    var form = await context.Request.ReadFormAsync();
    var providedKey = form["key"].FirstOrDefault();
    var rememberMe = form["rememberMe"].FirstOrDefault() == "true";
    
    if (string.IsNullOrWhiteSpace(providedKey))
    {
        context.Response.StatusCode = 400;
        await context.Response.WriteAsync("API ключ не предоставлен");
        return;
    }
    
    // Просто сравниваем ключ с эталоном
    if (providedKey != adminApiKey)
    {
        context.Response.StatusCode = 401;
        await context.Response.WriteAsync("Неверный API ключ");
        return;
    }
    
    // Ключ валиден - устанавливаем cookie
    // Используем Path = "/" чтобы cookie работал через Nginx прокси
    // (Nginx проксирует /admin на корневой путь /, поэтому cookie должен быть доступен на корневом пути)
    var cookieOptions = new Microsoft.AspNetCore.Http.CookieOptions
    {
        HttpOnly = true,
        Secure = false, // Установите true для HTTPS
        SameSite = Microsoft.AspNetCore.Http.SameSiteMode.Lax,
        Path = "/" // Всегда используем корневой путь для cookie
    };
    
    if (rememberMe)
    {
        cookieOptions.MaxAge = TimeSpan.FromDays(30);
    }
    // Если rememberMe = false, не устанавливаем MaxAge - это будет session cookie
    
    context.Response.Cookies.Append("weddy_admin_key", providedKey, cookieOptions);
    
    // Возвращаем JSON с путем для редиректа
    // Определяем базовый путь из заголовка X-Forwarded-Prefix
    var prefix = context.Request.Headers["X-Forwarded-Prefix"].FirstOrDefault() ?? "";
    var redirectPath = string.IsNullOrEmpty(prefix) ? "/" : prefix;
    context.Response.StatusCode = 200;
    context.Response.ContentType = "application/json";
    await context.Response.WriteAsync($"{{\"redirect\": \"{redirectPath}\"}}");
});

// Login page - только форма ввода ключа (для прямого доступа к /login)
app.MapGet("/login", async (HttpContext context) =>
{
    // Проверяем, есть ли уже валидный ключ в cookie
    var providedKey = context.Request.Cookies["weddy_admin_key"];
    if (!string.IsNullOrEmpty(providedKey) && providedKey == adminApiKey)
    {
        // Уже авторизован - редирект на главную
        // Определяем базовый путь из заголовка X-Forwarded-Prefix
        var prefix = context.Request.Headers["X-Forwarded-Prefix"].FirstOrDefault() ?? "";
        var homePath = string.IsNullOrEmpty(prefix) ? "/" : prefix;
        context.Response.Redirect(homePath, permanent: false);
        return Results.Empty;
    }
    
    // Показываем форму логина
    var loginHtml = GetLoginHtml();
    context.Response.ContentType = "text/html";
    context.Response.Headers.Append("Cache-Control", "no-cache, no-store, must-revalidate, max-age=0");
    return Results.Content(loginHtml);
});

// Admin UI Route - отдаем HTML админки или редирект на login (для / и /admin)
app.MapGet("/", async (HttpContext context) =>
{
    try
    {
        // Проверяем ключ только из cookie (безопасно)
        var providedKey = context.Request.Cookies["weddy_admin_key"];
        
        if (string.IsNullOrEmpty(providedKey) || providedKey != adminApiKey)
        {
            // Ключ не предоставлен или неверный - редирект на страницу логина
            // Определяем базовый путь из заголовка X-Forwarded-Prefix для правильного редиректа
            var prefix = context.Request.Headers["X-Forwarded-Prefix"].FirstOrDefault() ?? "";
            var loginPath = string.IsNullOrEmpty(prefix) ? "login" : $"{prefix}/login";
            context.Response.Redirect(loginPath, permanent: false);
            return Results.Empty;
        }
        
        // Ключ валиден - отдаем полный HTML админки
        // Определяем путь к файлу
        var webRootPath = app.Environment.WebRootPath;
        if (string.IsNullOrEmpty(webRootPath))
        {
            webRootPath = Path.Combine(AppContext.BaseDirectory, "wwwroot");
        }
        
        var htmlPath = Path.Combine(webRootPath, "index.html");
        
        // Если файл не найден, пробуем альтернативные пути
        if (!File.Exists(htmlPath))
        {
            var altPaths = new[]
            {
                Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "index.html"),
                Path.Combine(AppContext.BaseDirectory, "index.html"),
                Path.Combine(Directory.GetCurrentDirectory(), "index.html")
            };
            
            foreach (var altPath in altPaths)
            {
                if (File.Exists(altPath))
                {
                    htmlPath = altPath;
                    break;
                }
            }
        }
        
        if (!File.Exists(htmlPath))
        {
            context.Response.StatusCode = 500;
            var errorMsg = $"HTML file not found. WebRootPath: {webRootPath}, BaseDirectory: {AppContext.BaseDirectory}, CurrentDirectory: {Directory.GetCurrentDirectory()}, Tried: {htmlPath}";
            await context.Response.WriteAsync(errorMsg);
            return Results.Empty;
        }
        
        var html = await File.ReadAllTextAsync(htmlPath);
        
        // Заменяем плейсхолдеры на реальные URL
        html = html.Replace("{{INVITATION_BASE_URL}}", invitationBaseUrl);
        html = html.Replace("{{API_BASE_URL}}", apiBaseUrl);
        
        context.Response.ContentType = "text/html";
        context.Response.Headers.Append("Cache-Control", "no-cache, no-store, must-revalidate, max-age=0");
        context.Response.Headers.Append("Pragma", "no-cache");
        context.Response.Headers.Append("Expires", "0");
        context.Response.Headers.Append("ETag", $"\"{DateTime.UtcNow.Ticks}\"");
        context.Response.Headers.Append("Last-Modified", DateTime.UtcNow.ToString("R"));
        return Results.Content(html);
    }
    catch (Exception ex)
    {
        context.Response.StatusCode = 500;
        await context.Response.WriteAsync($"Error: {ex.Message}\nStack: {ex.StackTrace}");
        return Results.Empty;
    }
});

// Logout endpoint - очищает cookie и редиректит на страницу входа (для /logout и /admin/logout)
app.MapGet("/logout", async (HttpContext context) =>
{
    // Удаляем cookie с корневым Path
    context.Response.Cookies.Delete("weddy_admin_key", new Microsoft.AspNetCore.Http.CookieOptions
    {
        Path = "/"
    });
    
    // Определяем базовый путь из заголовка X-Forwarded-Prefix для редиректа
    var prefix = context.Request.Headers["X-Forwarded-Prefix"].FirstOrDefault() ?? "";
    var loginPath = string.IsNullOrEmpty(prefix) ? "login" : $"{prefix}/login";
    context.Response.Redirect(loginPath, permanent: false);
    return Results.Empty;
});

// Обработка путей с префиксом /admin (когда Nginx передает полный путь)
// Используем один маршрут для /admin и /admin/ чтобы избежать конфликта маршрутов
app.MapGet("/admin", async (HttpContext context) =>
{
    try
    {
        // Проверяем cookie и показываем админку
        var providedKey = context.Request.Cookies["weddy_admin_key"];
        if (string.IsNullOrEmpty(providedKey) || providedKey != adminApiKey)
        {
            context.Response.Redirect("/admin/login", permanent: false);
            return Results.Empty;
        }
        
        // Ключ валиден - отдаем полный HTML админки
        // Определяем путь к файлу
        var webRootPath = app.Environment.WebRootPath;
        if (string.IsNullOrEmpty(webRootPath))
        {
            webRootPath = Path.Combine(AppContext.BaseDirectory, "wwwroot");
        }
        
        var htmlPath = Path.Combine(webRootPath, "index.html");
        
        // Если файл не найден, пробуем альтернативные пути
        if (!File.Exists(htmlPath))
        {
            var altPaths = new[]
            {
                Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "index.html"),
                Path.Combine(AppContext.BaseDirectory, "index.html"),
                Path.Combine(Directory.GetCurrentDirectory(), "index.html")
            };
            
            foreach (var altPath in altPaths)
            {
                if (File.Exists(altPath))
                {
                    htmlPath = altPath;
                    break;
                }
            }
        }
        
        if (!File.Exists(htmlPath))
        {
            context.Response.StatusCode = 500;
            var errorMsg = $"HTML file not found. WebRootPath: {webRootPath}, BaseDirectory: {AppContext.BaseDirectory}, CurrentDirectory: {Directory.GetCurrentDirectory()}, Tried: {htmlPath}";
            await context.Response.WriteAsync(errorMsg);
            return Results.Empty;
        }
        
        var html = await File.ReadAllTextAsync(htmlPath);
        html = html.Replace("{{INVITATION_BASE_URL}}", invitationBaseUrl);
        html = html.Replace("{{API_BASE_URL}}", apiBaseUrl);
        
        context.Response.ContentType = "text/html";
        context.Response.Headers.Append("Cache-Control", "no-cache, no-store, must-revalidate, max-age=0");
        context.Response.Headers.Append("Pragma", "no-cache");
        context.Response.Headers.Append("Expires", "0");
        context.Response.Headers.Append("ETag", $"\"{DateTime.UtcNow.Ticks}\"");
        context.Response.Headers.Append("Last-Modified", DateTime.UtcNow.ToString("R"));
        return Results.Content(html);
    }
    catch (Exception ex)
    {
        context.Response.StatusCode = 500;
        await context.Response.WriteAsync($"Error: {ex.Message}\nStack: {ex.StackTrace}");
        return Results.Empty;
    }
});

app.MapGet("/admin/login", async (HttpContext context) =>
{
    // Проверяем, есть ли уже валидный ключ в cookie
    var providedKey = context.Request.Cookies["weddy_admin_key"];
    if (!string.IsNullOrEmpty(providedKey) && providedKey == adminApiKey)
    {
        // Уже авторизован - редирект на главную
        context.Response.Redirect("/admin", permanent: false);
        return Results.Empty;
    }
    
    // Показываем форму логина
    var loginHtml = GetLoginHtml();
    context.Response.ContentType = "text/html";
    context.Response.Headers.Append("Cache-Control", "no-cache, no-store, must-revalidate, max-age=0");
    return Results.Content(loginHtml);
});

app.MapPost("/admin/login", async (HttpContext context) =>
{
    var form = await context.Request.ReadFormAsync();
    var providedKey = form["key"].FirstOrDefault();
    var rememberMe = form["rememberMe"].FirstOrDefault() == "true";
    
    if (string.IsNullOrWhiteSpace(providedKey))
    {
        context.Response.StatusCode = 400;
        await context.Response.WriteAsync("API ключ не предоставлен");
        return;
    }
    
    if (providedKey != adminApiKey)
    {
        context.Response.StatusCode = 401;
        await context.Response.WriteAsync("Неверный API ключ");
        return;
    }
    
    var cookieOptions = new Microsoft.AspNetCore.Http.CookieOptions
    {
        HttpOnly = true,
        Secure = false,
        SameSite = Microsoft.AspNetCore.Http.SameSiteMode.Lax,
        Path = "/"
    };
    
    if (rememberMe)
    {
        cookieOptions.MaxAge = TimeSpan.FromDays(30);
    }
    
    context.Response.Cookies.Append("weddy_admin_key", providedKey, cookieOptions);
    context.Response.StatusCode = 200;
    context.Response.ContentType = "application/json";
    await context.Response.WriteAsync("{\"redirect\": \"/admin/\"}");
});

app.MapGet("/admin/logout", async (HttpContext context) =>
{
    context.Response.Cookies.Delete("weddy_admin_key", new Microsoft.AspNetCore.Http.CookieOptions
    {
        Path = "/"
    });
    context.Response.Redirect("/admin/login", permanent: false);
    return Results.Empty;
});

// Static files отключены - HTML отдается через MapGet с заменой плейсхолдеров
// app.UseStaticFiles();
// app.UseDefaultFiles();

app.Run();
