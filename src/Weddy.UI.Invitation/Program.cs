var builder = WebApplication.CreateBuilder(args);

// Configuration
var apiBaseUrl = builder.Configuration["ApiBaseUrl"] ?? "http://localhost:5000";
var baseUrl = builder.Configuration["BaseUrl"] ?? "http://localhost:5002";

builder.Services.AddHttpClient();
builder.Services.AddCors();

var app = builder.Build();

// Static files
app.UseStaticFiles();
app.UseDefaultFiles();

// CORS для работы с API
app.UseCors(policy => policy
    .AllowAnyOrigin()
    .AllowAnyMethod()
    .AllowAnyHeader());

// Invitation Routes
app.MapGet("/i/{token}", (string token, HttpContext context) => 
{
    var html = Weddy.UI.Invitation.Pages.InvitationPage.GetHtml(token, baseUrl);
    context.Response.Headers.Append("Cache-Control", "no-cache, no-store, must-revalidate, max-age=0");
    context.Response.Headers.Append("Pragma", "no-cache");
    context.Response.Headers.Append("Expires", "0");
    context.Response.Headers.Append("Last-Modified", DateTime.UtcNow.ToString("R"));
    context.Response.Headers.Append("ETag", $"\"{DateTime.UtcNow.Ticks}\"");
    return Results.Content(html, "text/html");
});

// API Proxy для карточек приглашений
app.MapGet("/api/{**path}", async (string path, HttpContext context, IHttpClientFactory httpClientFactory) =>
{
    var client = httpClientFactory.CreateClient();
    var apiUrl = $"{apiBaseUrl}/{path}";
    
    var request = new HttpRequestMessage(HttpMethod.Get, apiUrl);
    foreach (var header in context.Request.Headers)
    {
        if (header.Key.StartsWith("X-") || header.Key.Equals("Authorization", StringComparison.OrdinalIgnoreCase))
        {
            request.Headers.TryAddWithoutValidation(header.Key, header.Value.ToArray());
        }
    }
    
    var response = await client.SendAsync(request);
    var content = await response.Content.ReadAsStringAsync();
    
    context.Response.StatusCode = (int)response.StatusCode;
    context.Response.ContentType = response.Content.Headers.ContentType?.ToString() ?? "application/json";
    await context.Response.WriteAsync(content);
});

app.MapPut("/api/{**path}", async (string path, HttpContext context, IHttpClientFactory httpClientFactory) =>
{
    var client = httpClientFactory.CreateClient();
    var apiUrl = $"{apiBaseUrl}/{path}";
    
    using var reader = new StreamReader(context.Request.Body);
    var body = await reader.ReadToEndAsync();
    
    var request = new HttpRequestMessage(HttpMethod.Put, apiUrl)
    {
        Content = new StringContent(body, System.Text.Encoding.UTF8, "application/json")
    };
    
    foreach (var header in context.Request.Headers)
    {
        if (header.Key.StartsWith("X-") || header.Key.Equals("Authorization", StringComparison.OrdinalIgnoreCase))
        {
            request.Headers.TryAddWithoutValidation(header.Key, header.Value.ToArray());
        }
    }
    
    var response = await client.SendAsync(request);
    var content = await response.Content.ReadAsStringAsync();
    
    context.Response.StatusCode = (int)response.StatusCode;
    context.Response.ContentType = response.Content.Headers.ContentType?.ToString() ?? "application/json";
    await context.Response.WriteAsync(content);
});

app.Run();
