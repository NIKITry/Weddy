namespace Weddy.Domain.Entities;

public class Invitation
{
    public Guid Id { get; set; }
    public string Token { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string InvitationText { get; set; } = string.Empty;
    public InvitationStatus Status { get; set; } = InvitationStatus.None;
    public string Note { get; set; } = string.Empty;
    public string MetaNote { get; set; } = string.Empty; // Мета-информация для админа
    public bool Archived { get; set; } = false;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

