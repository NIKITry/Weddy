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

// Login page - только форма ввода ключа
app.MapGet("/login", async (HttpContext context) =>
{
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
<body class=""bg-gradient-to-br from-pink-50 to-purple-50 min-h-screen flex items-center justify-center"">
    <div class=""bg-white rounded-lg shadow-lg p-8 max-w-md w-full"" x-data=""loginApp()"">
        <h1 class=""text-3xl font-bold text-center mb-6 text-purple-600"">🔐 Админ-панель</h1>
        <div class=""space-y-4"">
            <div>
                <label class=""block text-sm font-medium text-gray-700 mb-2"">API ключ</label>
                <input 
                    x-model=""adminKeyInput""
                    @keyup.enter=""login()""
                    type=""password"" 
                    placeholder=""Введите API ключ""
                    class=""w-full p-3 border rounded-lg focus:ring-2 focus:ring-pink-500"">
            </div>
            <div class=""flex items-center"">
                <input 
                    x-model=""rememberMe""
                    type=""checkbox"" 
                    id=""rememberMe""
                    class=""h-4 w-4 text-purple-600 focus:ring-purple-500 border-gray-300 rounded"">
                <label for=""rememberMe"" class=""ml-2 block text-sm text-gray-700"">
                    Запомнить меня
                </label>
            </div>
            <button 
                @click=""login()""
                class=""w-full bg-purple-500 hover:bg-purple-600 text-white px-6 py-3 rounded-lg font-semibold"">
                Войти
            </button>
            <div x-show=""errorMessage"" class=""text-red-600 text-sm text-center"" x-text=""errorMessage""></div>
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
                    // Проверяем ключ через API
                    try {
                        const response = await fetch('" + apiBaseUrl + @"/admin/event', {
                            headers: { 'X-Admin-Key': this.adminKeyInput.trim() }
                        });
                        if (response.ok) {
                            // Ключ валиден, сохраняем и редиректим
                            if (this.rememberMe) {
                                localStorage.setItem('weddy_admin_key', this.adminKeyInput.trim());
                            } else {
                                sessionStorage.setItem('weddy_admin_key', this.adminKeyInput.trim());
                            }
                            // Редирект на админку с ключом в query параметре
                            // Через nginx путь будет /admin/, поэтому используем относительный путь
                            window.location.href = '/?key=' + encodeURIComponent(this.adminKeyInput.trim());
                        } else {
                            this.errorMessage = 'Неверный API ключ';
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
    // Проверяем ключ из query параметра или cookie
    var providedKey = context.Request.Query["key"].FirstOrDefault() 
        ?? context.Request.Cookies["weddy_admin_key"];
    
    if (string.IsNullOrEmpty(providedKey) || providedKey != adminApiKey)
    {
        // Ключ не предоставлен или неверный - редирект на страницу входа
        context.Response.Redirect("/login");
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
        
        // Устанавливаем cookie для последующих запросов (если ключ был в query)
        if (context.Request.Query.ContainsKey("key"))
        {
            context.Response.Cookies.Append("weddy_admin_key", providedKey, new Microsoft.AspNetCore.Http.CookieOptions
            {
                HttpOnly = true,
                Secure = false, // Установите true для HTTPS
                SameSite = Microsoft.AspNetCore.Http.SameSiteMode.Lax,
                MaxAge = TimeSpan.FromDays(30) // Или используйте Expires
            });
        }
        
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
    context.Response.Cookies.Delete("weddy_admin_key");
    context.Response.Redirect("/login");
    return Results.Empty;
});

// Static files отключены - HTML отдается через MapGet с заменой плейсхолдеров
// app.UseStaticFiles();
// app.UseDefaultFiles();

app.Run();
