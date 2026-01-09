using Weddy.Domain.Entities;

namespace Weddy.Application.DTOs;

public class PublicInvitationResponse
{
    public string DisplayName { get; set; } = string.Empty;
    public string InvitationText { get; set; } = string.Empty;
    public List<EventPlanItem>? EventPlan { get; set; }
    public InvitationStatus Status { get; set; }
    public string Note { get; set; } = string.Empty;
    public EventInfo Event { get; set; } = new();
}

public class EventInfo
{
    public string InvitationText { get; set; } = string.Empty;
    public DateTime? EventDate { get; set; }
    public List<EventPlanItem>? EventPlan { get; set; }
    public string FooterNote { get; set; } = string.Empty;
    public string CoupleDisplayName { get; set; } = string.Empty;
}

