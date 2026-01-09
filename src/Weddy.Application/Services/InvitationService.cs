using System.Text.Json;
using Dapper;
using Weddy.Application.Database;
using Weddy.Application.DTOs;
using Weddy.Domain.Entities;

namespace Weddy.Application.Services;

public interface IInvitationService
{
    Task<Invitation?> GetByTokenAsync(string token, CancellationToken cancellationToken = default);
    Task<Invitation?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<InvitationListResponse> GetListAsync(
        string? status = null,
        string? query = null,
        bool? archived = null,
        CancellationToken cancellationToken = default);
    Task<Invitation> CreateAsync(Invitation invitation, CancellationToken cancellationToken = default);
    Task<Invitation> UpdateAsync(Invitation invitation, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}

public class InvitationService : IInvitationService
{
    private readonly IDbConnectionFactory _connectionFactory;

    public InvitationService(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<Invitation?> GetByTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            SELECT id AS Id, token AS Token, display_name AS DisplayName, invitation_text AS InvitationText,
                   status AS Status, note AS Note, meta_note AS MetaNote, archived AS Archived, created_at AS CreatedAt, updated_at AS UpdatedAt
            FROM invitations
            WHERE token = @Token AND archived = false";
        
        var result = await connection.QueryFirstOrDefaultAsync<Invitation>(
            new CommandDefinition(sql, new { Token = token }, cancellationToken: cancellationToken));
        
        return result;
    }

    public async Task<Invitation?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            SELECT id AS Id, token AS Token, display_name AS DisplayName, invitation_text AS InvitationText,
                   status AS Status, note AS Note, meta_note AS MetaNote, archived AS Archived, created_at AS CreatedAt, updated_at AS UpdatedAt
            FROM invitations
            WHERE id = @Id";
        
        var result = await connection.QueryFirstOrDefaultAsync<Invitation>(
            new CommandDefinition(sql, new { Id = id }, cancellationToken: cancellationToken));
        
        return result;
    }

    public async Task<InvitationListResponse> GetListAsync(
        string? status = null,
        string? query = null,
        bool? archived = null,
        CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        
        var conditions = new List<string> { "1=1" };
        var parameters = new DynamicParameters();

        if (!string.IsNullOrWhiteSpace(status))
        {
            conditions.Add("status = @Status::invitation_status");
            parameters.Add("Status", status.ToLower());
        }

        if (!string.IsNullOrWhiteSpace(query))
        {
            conditions.Add("(display_name ILIKE @Query OR meta_note ILIKE @Query)");
            parameters.Add("Query", $"%{query}%");
        }

        if (archived.HasValue)
        {
            conditions.Add("archived = @Archived");
            parameters.Add("Archived", archived.Value);
        }

        var whereClause = string.Join(" AND ", conditions);
        var sql = $@"
            SELECT id AS Id, token AS Token, display_name AS DisplayName, invitation_text AS InvitationText,
                   status AS Status, note AS Note, meta_note AS MetaNote, archived AS Archived, created_at AS CreatedAt, updated_at AS UpdatedAt
            FROM invitations
            WHERE {whereClause}
            ORDER BY created_at DESC";

        var items = (await connection.QueryAsync<Invitation>(
            new CommandDefinition(sql, parameters, cancellationToken: cancellationToken))).ToList();

        var countSql = $"SELECT COUNT(*) FROM invitations WHERE {whereClause}";
        var total = await connection.QuerySingleAsync<int>(
            new CommandDefinition(countSql, parameters, cancellationToken: cancellationToken));

        return new InvitationListResponse
        {
            Items = items.Select(i => new InvitationDto
            {
                Id = i.Id,
                Token = i.Token,
                DisplayName = i.DisplayName,
                InvitationText = i.InvitationText,
                InvitationDate = null, // Дата мероприятия теперь в event settings
                EventPlan = null, // План мероприятия теперь в event settings
                Status = i.Status,
                Note = i.Note,
                MetaNote = i.MetaNote,
                Archived = i.Archived,
                CreatedAt = i.CreatedAt,
                UpdatedAt = i.UpdatedAt
            }).ToList(),
            Total = total
        };
    }

    public async Task<Invitation> CreateAsync(Invitation invitation, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
            const string sql = @"
            INSERT INTO invitations (id, token, display_name, invitation_text, status, note, meta_note, archived, created_at, updated_at)
            VALUES (@Id, @Token, @DisplayName, @InvitationText, @Status::invitation_status, @Note, @MetaNote, @Archived, @CreatedAt, @UpdatedAt)
            RETURNING id AS Id, token AS Token, display_name AS DisplayName, invitation_text AS InvitationText,
                      status AS Status, note AS Note, meta_note AS MetaNote, archived AS Archived, created_at AS CreatedAt, updated_at AS UpdatedAt";
        
        var parameters = new
        {
            invitation.Id,
            invitation.Token,
            invitation.DisplayName,
            invitation.InvitationText,
            Status = invitation.Status.ToString().ToLower(),
            invitation.Note,
            invitation.MetaNote,
            invitation.Archived,
            invitation.CreatedAt,
            invitation.UpdatedAt
        };
        
        var result = await connection.QuerySingleAsync<Invitation>(
            new CommandDefinition(sql, parameters, cancellationToken: cancellationToken));
        
        return result;
    }

    public async Task<Invitation> UpdateAsync(Invitation invitation, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            UPDATE invitations
            SET display_name = @DisplayName,
                invitation_text = @InvitationText,
                status = @Status::invitation_status,
                note = @Note,
                meta_note = @MetaNote,
                archived = @Archived,
                updated_at = @UpdatedAt
            WHERE id = @Id
            RETURNING id AS Id, token AS Token, display_name AS DisplayName, invitation_text AS InvitationText,
                      status AS Status, note AS Note, meta_note AS MetaNote, archived AS Archived, created_at AS CreatedAt, updated_at AS UpdatedAt";
        
        var parameters = new
        {
            invitation.Id,
            invitation.DisplayName,
            invitation.InvitationText,
            Status = invitation.Status.ToString().ToLower(),
            invitation.Note,
            invitation.MetaNote,
            invitation.Archived,
            invitation.UpdatedAt
        };
        
        var result = await connection.QuerySingleAsync<Invitation>(
            new CommandDefinition(sql, parameters, cancellationToken: cancellationToken));
        
        return result;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            DELETE FROM invitations
            WHERE id = @Id";
        
        var rowsAffected = await connection.ExecuteAsync(
            new CommandDefinition(sql, new { Id = id }, cancellationToken: cancellationToken));
        
        return rowsAffected > 0;
    }

    private static List<EventPlanItem>? ParseEventPlan(string? json)
    {
        if (string.IsNullOrWhiteSpace(json) || json == "[]" || json == "null")
            return null;

        try
        {
            var result = JsonSerializer.Deserialize<List<EventPlanItem>>(json);
            if (result == null || result.Count == 0)
                return null;

            // Фильтруем пустые пункты (где все поля пустые)
            var validItems = result
                .Where(item => !string.IsNullOrWhiteSpace(item.Time) || 
                             !string.IsNullOrWhiteSpace(item.Title) || 
                             !string.IsNullOrWhiteSpace(item.Location))
                .ToList();

            return validItems.Count > 0 ? validItems : null;
        }
        catch (JsonException)
        {
            // Если не удалось распарсить как JSON, возвращаем null
            return null;
        }
        catch
        {
            return null;
        }
    }
}

