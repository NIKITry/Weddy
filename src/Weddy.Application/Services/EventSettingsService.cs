using Dapper;
using Weddy.Application.Database;
using Weddy.Domain.Entities;

namespace Weddy.Application.Services;

public interface IEventSettingsService
{
    Task<EventSettings?> GetAsync(CancellationToken cancellationToken = default);
    Task<EventSettings> CreateOrUpdateAsync(
        EventSettings settings,
        CancellationToken cancellationToken = default);
}

public class EventSettingsService : IEventSettingsService
{
    private readonly IDbConnectionFactory _connectionFactory;

    public EventSettingsService(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<EventSettings?> GetAsync(CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            SELECT id AS Id, invitation_text AS InvitationText, wedding_couple_name AS WeddingCoupleName,
                   couple_display_name AS CoupleDisplayName, event_date AS EventDate, event_plan AS EventPlanJson, 
                   footer_note AS FooterNote, created_at AS CreatedAt, updated_at AS UpdatedAt
            FROM event_settings
            ORDER BY created_at DESC
            LIMIT 1";
        
        var result = await connection.QueryFirstOrDefaultAsync<EventSettings>(
            new CommandDefinition(sql, cancellationToken: cancellationToken));
        
        return result;
    }

    public async Task<EventSettings> CreateOrUpdateAsync(
        EventSettings settings,
        CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        
        // Проверяем, существует ли запись
        var existing = await GetAsync(cancellationToken);
        
        if (existing == null)
        {
            // Создаём новую запись
            const string insertSql = @"
                INSERT INTO event_settings (id, invitation_text, wedding_couple_name, couple_display_name, event_date, event_plan, footer_note, created_at, updated_at)
                VALUES (@Id, @InvitationText, @WeddingCoupleName, @CoupleDisplayName, @EventDate, @EventPlanJson::jsonb, @FooterNote, @CreatedAt, @UpdatedAt)
                RETURNING id AS Id, invitation_text AS InvitationText, wedding_couple_name AS WeddingCoupleName,
                          couple_display_name AS CoupleDisplayName, event_date AS EventDate, event_plan AS EventPlanJson, 
                          footer_note AS FooterNote, created_at AS CreatedAt, updated_at AS UpdatedAt";
            
            var result = await connection.QuerySingleAsync<EventSettings>(
                new CommandDefinition(insertSql, settings, cancellationToken: cancellationToken));
            
            return result;
        }
        else
        {
            // Обновляем существующую
            const string updateSql = @"
                UPDATE event_settings
                SET invitation_text = @InvitationText,
                    wedding_couple_name = @WeddingCoupleName,
                    couple_display_name = @CoupleDisplayName,
                    event_date = @EventDate,
                    event_plan = @EventPlanJson::jsonb,
                    footer_note = @FooterNote,
                    updated_at = @UpdatedAt
                WHERE id = @Id
                RETURNING id AS Id, invitation_text AS InvitationText, wedding_couple_name AS WeddingCoupleName,
                          couple_display_name AS CoupleDisplayName, event_date AS EventDate, event_plan AS EventPlanJson, 
                          footer_note AS FooterNote, created_at AS CreatedAt, updated_at AS UpdatedAt";
            
            var result = await connection.QuerySingleAsync<EventSettings>(
                new CommandDefinition(updateSql, settings, cancellationToken: cancellationToken));
            
            return result;
        }
    }
}

