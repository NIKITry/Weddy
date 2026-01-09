namespace Weddy.Domain.Entities;

public class EventSettings
{
    public Guid Id { get; set; }
    public string InvitationText { get; set; } = string.Empty;
    public string WeddingCoupleName { get; set; } = string.Empty;
    public string CoupleDisplayName { get; set; } = string.Empty; // Имена пары для отображения "Ваши Никита и Виолетта"
    public DateTime? EventDate { get; set; }
    public string EventPlanJson { get; set; } = string.Empty;
    public string FooterNote { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

