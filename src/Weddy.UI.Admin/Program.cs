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

var app = builder.Build();

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
    var cookieOptions = new Microsoft.AspNetCore.Http.CookieOptions
    {
        HttpOnly = true,
        Secure = false, // Установите true для HTTPS
        SameSite = Microsoft.AspNetCore.Http.SameSiteMode.Lax,
        Path = "/"
    };
    
    if (rememberMe)
    {
        cookieOptions.MaxAge = TimeSpan.FromDays(30);
    }
    // Если rememberMe = false, не устанавливаем MaxAge - это будет session cookie
    
    context.Response.Cookies.Append("weddy_admin_key", providedKey, cookieOptions);
    context.Response.StatusCode = 200;
    await context.Response.WriteAsync("OK");
});

// Login page - только форма ввода ключа
app.MapGet("/login", async (HttpContext context) =>
{
    // Проверяем, есть ли уже валидный ключ в cookie
    var providedKey = context.Request.Cookies["weddy_admin_key"];
    if (!string.IsNullOrEmpty(providedKey) && providedKey == adminApiKey)
    {
        // Уже авторизован - редирект на главную
        context.Response.Redirect("/");
        return Results.Empty;
    }
    
    var loginHtml = @"
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
                        
                        const response = await fetch('login', {
                            method: 'POST',
                            body: formData,
                            credentials: 'same-origin'
                        });
                        
                        if (response.status === 200) {
                            setTimeout(() => {
                                window.location.href = window.location.pathname.replace(/\/login$/, '') || '/';
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
    
        context.Response.ContentType = "text/html";
    context.Response.Headers.Append("Cache-Control", "no-cache, no-store, must-revalidate, max-age=0");
    return Results.Content(loginHtml);
});

// Admin UI Route - отдаем HTML только после проверки ключа
app.MapGet("/", async (HttpContext context) =>
{
    // Проверяем ключ только из cookie (безопасно)
    var providedKey = context.Request.Cookies["weddy_admin_key"];
    
    if (string.IsNullOrEmpty(providedKey) || providedKey != adminApiKey)
    {
        // Ключ не предоставлен или неверный - редирект на страницу входа
        // Используем относительный путь, чтобы работало через Nginx
        context.Response.Redirect("login");
        return Results.Empty;
    }
    
    // Ключ валиден - отдаем полный HTML админки
    var htmlPath = Path.Combine(app.Environment.WebRootPath ?? "wwwroot", "index.html");
    if (File.Exists(htmlPath))
    {
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
    return Results.NotFound();
});

// Logout endpoint - очищает cookie и редиректит на страницу входа
app.MapGet("/logout", async (HttpContext context) =>
{
    // Удаляем cookie с явным указанием Path
    context.Response.Cookies.Delete("weddy_admin_key", new Microsoft.AspNetCore.Http.CookieOptions
    {
        Path = "/"
    });
    // Используем относительный путь, чтобы работало через Nginx
    context.Response.Redirect("login");
    return Results.Empty;
});

// Static files отключены - HTML отдается через MapGet с заменой плейсхолдеров
// app.UseStaticFiles();
// app.UseDefaultFiles();

app.Run();
