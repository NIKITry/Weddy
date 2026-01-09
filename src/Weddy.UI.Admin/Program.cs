var builder = WebApplication.CreateBuilder(args);

// Configuration
var apiBaseUrl = builder.Configuration["ApiBaseUrl"] ?? "http://localhost:5000";
var invitationBaseUrl = builder.Configuration["InvitationBaseUrl"] ?? "http://localhost:5002";

var app = builder.Build();

// Admin UI Route - отдаем HTML с заменой плейсхолдеров
app.MapGet("/", async (HttpContext context) =>
{
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

// Static files отключены - HTML отдается через MapGet с заменой плейсхолдеров
// app.UseStaticFiles();
// app.UseDefaultFiles();

app.Run();
