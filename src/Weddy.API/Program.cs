using Microsoft.OpenApi.Models;
using Weddy.Application.Database;
using Weddy.Application.DTOs;
using Weddy.Application.Services;
using Weddy.Domain.Entities;
using Weddy.Infrastructure.Database;
using Weddy.Infrastructure.Services;

// Настройка Dapper
DapperConfig.Configure();

var builder = WebApplication.CreateBuilder(args);

// Configuration
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
var adminKey = builder.Configuration["Admin:ApiKey"] 
    ?? throw new InvalidOperationException("Admin:ApiKey not found in configuration.");
var baseUrl = builder.Configuration["BaseUrl"] ?? "https://your-domain";

// Services
builder.Services.AddSingleton<IDbConnectionFactory>(_ => new DbConnectionFactory(connectionString));
builder.Services.AddScoped<IInvitationService, InvitationService>();
builder.Services.AddScoped<IEventSettingsService, EventSettingsService>();
builder.Services.AddScoped<ITokenGenerator, TokenGenerator>();

// CORS
var corsOrigins = builder.Configuration["Cors:Origins"]?.Split(',', StringSplitOptions.RemoveEmptyEntries) 
    ?? new[] { "http://localhost:5001", "http://localhost:5089" };
    
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins(corsOrigins)
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

// JSON serialization (camelCase)
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
    options.SerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Weddy RSVP API",
        Version = "v1",
        Description = "API для управления свадебными приглашениями и RSVP"
    });
    
    // Добавляем схему для X-Admin-Key
    c.AddSecurityDefinition("AdminKey", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.ApiKey,
        In = ParameterLocation.Header,
        Name = "X-Admin-Key",
        Description = "API ключ для доступа к административным эндпоинтам"
    });
    
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "AdminKey"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// Middleware
app.UseCors();
app.UseSwagger();
app.UseSwaggerUI();

// Admin Key Middleware
app.Use(async (context, next) =>
{
    if (context.Request.Path.StartsWithSegments("/admin"))
    {
        var providedKey = context.Request.Headers["X-Admin-Key"].FirstOrDefault();
        if (providedKey != adminKey)
        {
            context.Response.StatusCode = 401;
            await context.Response.WriteAsync("Unauthorized: Invalid or missing X-Admin-Key");
            return;
        }
    }
    await next();
});

// Public Endpoints
app.MapGet("/public/invitations/{token}", async (
    string token,
    IInvitationService invitationService,
    IEventSettingsService eventService) =>
{
    var invitation = await invitationService.GetByTokenAsync(token);
    if (invitation == null)
    {
        return Results.NotFound();
    }

    var eventSettings = await eventService.GetAsync();
    if (eventSettings == null)
    {
        return Results.Problem("Event settings not found", statusCode: 500);
    }

    // Используем индивидуальный текст приглашения, если он есть, иначе общий
    var invitationText = !string.IsNullOrWhiteSpace(invitation.InvitationText) 
        ? invitation.InvitationText 
        : eventSettings.InvitationText;

    // План мероприятия берется из event settings (общий для всего события)
    var eventPlan = !string.IsNullOrWhiteSpace(eventSettings.EventPlanJson) && eventSettings.EventPlanJson != "[]"
        ? System.Text.Json.JsonSerializer.Deserialize<List<EventPlanItem>>(eventSettings.EventPlanJson)
        : null;

    return Results.Ok(new PublicInvitationResponse
    {
        DisplayName = invitation.DisplayName,
        InvitationText = invitationText,
        EventPlan = eventPlan,
        Status = invitation.Status,
        Note = invitation.Note,
        Event = new EventInfo
        {
            InvitationText = invitationText,
            EventDate = eventSettings.EventDate,
            EventPlan = eventPlan,
            FooterNote = eventSettings.FooterNote ?? string.Empty,
            CoupleDisplayName = eventSettings.CoupleDisplayName ?? string.Empty
        }
    });
})
.WithName("GetPublicInvitation")
.WithTags("Public")
.WithOpenApi();

app.MapPut("/public/invitations/{token}/rsvp", async (
    string token,
    RsvpRequest request,
    IInvitationService invitationService) =>
{
    // Валидация статуса
    if (request.Status == InvitationStatus.None)
    {
        return Results.BadRequest(new { error = "Status cannot be 'none'. Use 'yes' or 'no'." });
    }

    var invitation = await invitationService.GetByTokenAsync(token);
    if (invitation == null)
    {
        return Results.NotFound();
    }

    invitation.Status = request.Status;
    invitation.Note = request.Note ?? string.Empty;
    invitation.UpdatedAt = DateTime.UtcNow;

    var updated = await invitationService.UpdateAsync(invitation);
    
    return Results.Ok(new PublicInvitationResponse
    {
        DisplayName = updated.DisplayName,
        Status = updated.Status,
        Note = updated.Note
    });
})
.WithName("UpdateRsvp")
.WithTags("Public")
.WithOpenApi();

// Admin Endpoints
app.MapGet("/admin/event", async (IEventSettingsService eventService) =>
{
    var settings = await eventService.GetAsync();
    if (settings == null)
    {
        return Results.Ok(new { invitation_text = string.Empty, event_date = (DateTime?)null, event_plan = new List<EventPlanItem>(), footer_note = string.Empty, couple_names_for_token = string.Empty, couple_display_name = string.Empty });
    }

    var eventPlan = !string.IsNullOrWhiteSpace(settings.EventPlanJson) && settings.EventPlanJson != "[]"
        ? System.Text.Json.JsonSerializer.Deserialize<List<EventPlanItem>>(settings.EventPlanJson)
        : new List<EventPlanItem>();

    return Results.Ok(new { 
        invitation_text = settings.InvitationText, 
        event_date = settings.EventDate,
        event_plan = eventPlan ?? new List<EventPlanItem>(),
        footer_note = settings.FooterNote ?? string.Empty,
        couple_names_for_token = settings.WeddingCoupleName ?? string.Empty,
        couple_display_name = settings.CoupleDisplayName ?? string.Empty
    });
})
.WithName("GetEventSettings")
.WithTags("Admin")
.WithOpenApi();

app.MapPut("/admin/event", async (
    UpdateEventSettingsRequest request,
    IEventSettingsService eventService) =>
{
    var existing = await eventService.GetAsync();
    var settings = existing ?? new EventSettings
    {
        Id = Guid.NewGuid(),
        CreatedAt = DateTime.UtcNow
    };

    // Фильтруем пустые пункты плана
    var validEventPlan = request.EventPlan?
        .Where(item => !string.IsNullOrWhiteSpace(item.Time) || 
                      !string.IsNullOrWhiteSpace(item.Title) || 
                      !string.IsNullOrWhiteSpace(item.Location))
        .ToList();

    settings.InvitationText = request.InvitationText;
    settings.EventDate = request.EventDate;
    settings.EventPlanJson = validEventPlan != null && validEventPlan.Count > 0
        ? System.Text.Json.JsonSerializer.Serialize(validEventPlan)
        : "[]";
    settings.FooterNote = request.FooterNote ?? string.Empty;
    settings.WeddingCoupleName = request.CoupleNamesForToken ?? string.Empty;
    settings.CoupleDisplayName = request.CoupleDisplayName ?? string.Empty;
    settings.UpdatedAt = DateTime.UtcNow;

    var updated = await eventService.CreateOrUpdateAsync(settings);
    
    return Results.Ok(new { invitation_text = updated.InvitationText, event_date = updated.EventDate });
})
.WithName("UpdateEventSettings")
.WithTags("Admin")
.WithOpenApi();

app.MapPost("/admin/invitations", async (
    CreateInvitationRequest request,
    IInvitationService invitationService,
    IEventSettingsService eventService,
    ITokenGenerator tokenGenerator) =>
{
    // Получаем имена пары из настроек события для генерации красивого токена
    var eventSettings = await eventService.GetAsync();
    var coupleNames = eventSettings?.WeddingCoupleName;
    
    // Генерируем уникальный токен (проверяем на уникальность до 10 попыток)
    string token;
    int attempts = 0;
    const int maxAttempts = 10;
    do
    {
        token = tokenGenerator.GenerateToken(coupleNames);
        var existing = await invitationService.GetByTokenAsync(token);
        if (existing == null) break;
        attempts++;
    } while (attempts < maxAttempts);
    
    if (attempts >= maxAttempts)
    {
        return Results.Problem("Не удалось сгенерировать уникальный токен. Попробуйте еще раз.");
    }
    
    var invitation = new Invitation
    {
        Id = Guid.NewGuid(),
        Token = token,
        DisplayName = request.DisplayName,
        InvitationText = request.InvitationText ?? string.Empty,
        Status = InvitationStatus.None,
        Note = string.Empty,
        MetaNote = request.MetaNote ?? string.Empty,
        Archived = false,
        CreatedAt = DateTime.UtcNow,
        UpdatedAt = DateTime.UtcNow
    };

    var created = await invitationService.CreateAsync(invitation);
    
    return Results.Created($"/admin/invitations/{created.Id}", new
    {
        id = created.Id,
        display_name = created.DisplayName,
        token = created.Token,
        url = $"{baseUrl}/i/{created.Token}",
        status = created.Status.ToString().ToLower(),
        archived = created.Archived
    });
})
.WithName("CreateInvitation")
.WithTags("Admin")
.WithOpenApi();

app.MapGet("/admin/invitations", async (
    string? status,
    string? query,
    string? archived,
    IInvitationService invitationService) =>
{
    // Парсим archived из строки
    bool? archivedBool = null;
    if (!string.IsNullOrWhiteSpace(archived))
    {
        if (bool.TryParse(archived, out var parsed))
        {
            archivedBool = parsed;
        }
    }
    
    // Нормализуем статус - убираем пробелы и приводим к нижнему регистру
    string? normalizedStatus = null;
    if (!string.IsNullOrWhiteSpace(status))
    {
        normalizedStatus = status.Trim().ToLower();
    }
    
    // Передаем параметры в сервис
    var result = await invitationService.GetListAsync(normalizedStatus, query, archivedBool);
    return Results.Ok(result);
})
.WithName("GetInvitations")
.WithTags("Admin")
.WithOpenApi();

app.MapGet("/admin/invitations/{id:guid}", async (
    Guid id,
    IInvitationService invitationService) =>
{
    var invitation = await invitationService.GetByIdAsync(id);
    if (invitation == null)
    {
        return Results.NotFound();
    }

    return Results.Ok(new InvitationDto
    {
        Id = invitation.Id,
        Token = invitation.Token,
        DisplayName = invitation.DisplayName,
        InvitationText = invitation.InvitationText,
        InvitationDate = null, // Дата мероприятия теперь в event settings
        EventPlan = null, // План мероприятия теперь в event settings
        Status = invitation.Status,
        Note = invitation.Note,
        MetaNote = invitation.MetaNote,
        Archived = invitation.Archived,
        CreatedAt = invitation.CreatedAt,
        UpdatedAt = invitation.UpdatedAt
    });
})
.WithName("GetInvitation")
.WithTags("Admin")
.WithOpenApi();

app.MapPatch("/admin/invitations/{id:guid}", async (
    Guid id,
    UpdateInvitationRequest request,
    IInvitationService invitationService) =>
{
    var invitation = await invitationService.GetByIdAsync(id);
    if (invitation == null)
    {
        return Results.NotFound();
    }

    if (request.DisplayName != null)
    {
        invitation.DisplayName = request.DisplayName;
    }

    if (request.InvitationText != null)
    {
        invitation.InvitationText = request.InvitationText;
    }

    if (request.MetaNote != null)
    {
        invitation.MetaNote = request.MetaNote;
    }

    // EventDate теперь в event settings, не в invitation

    if (request.Archived.HasValue)
    {
        invitation.Archived = request.Archived.Value;
    }

    invitation.UpdatedAt = DateTime.UtcNow;

    var updated = await invitationService.UpdateAsync(invitation);
    
    return Results.Ok(new InvitationDto
    {
        Id = updated.Id,
        Token = updated.Token,
        DisplayName = updated.DisplayName,
        InvitationText = updated.InvitationText,
        InvitationDate = null, // Дата мероприятия теперь в event settings
        EventPlan = null, // План мероприятия теперь в event settings
        Status = updated.Status,
        Note = updated.Note,
        Archived = updated.Archived,
        CreatedAt = updated.CreatedAt,
        UpdatedAt = updated.UpdatedAt
    });
})
.WithName("UpdateInvitation")
.WithTags("Admin")
.WithOpenApi();

app.MapDelete("/admin/invitations/{id:guid}", async (
    Guid id,
    IInvitationService invitationService) =>
{
    var deleted = await invitationService.DeleteAsync(id);
    if (!deleted)
    {
        return Results.NotFound();
    }
    
    return Results.NoContent();
})
.WithName("DeleteInvitation")
.WithTags("Admin")
.WithOpenApi();

app.Run();

