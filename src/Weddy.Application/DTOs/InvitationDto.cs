using Weddy.Domain.Entities;

namespace Weddy.Application.DTOs;

public class InvitationDto
{
    public Guid Id { get; set; }
    public string Token { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string InvitationText { get; set; } = string.Empty;
    public DateTime? InvitationDate { get; set; }
    public List<EventPlanItem>? EventPlan { get; set; }
    public InvitationStatus Status { get; set; }
    public string Note { get; set; } = string.Empty;
    public string MetaNote { get; set; } = string.Empty;
    public bool Archived { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

