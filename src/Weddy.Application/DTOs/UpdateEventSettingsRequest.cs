using System.ComponentModel.DataAnnotations;

namespace Weddy.Application.DTOs;

public class UpdateEventSettingsRequest
{
    [Required]
    public string InvitationText { get; set; } = string.Empty;
    
    public DateTime? EventDate { get; set; }
    
    public List<EventPlanItem>? EventPlan { get; set; }
    
    public string? FooterNote { get; set; }
    
    /// <summary>
    /// Имена пары для генерации красивых токенов (например, "nikita violetta" или "nikita-violetta")
    /// </summary>
    public string? CoupleNamesForToken { get; set; }
    
    /// <summary>
    /// Имена пары для отображения на карточке (например, "Никита и Виолетта")
    /// </summary>
    public string? CoupleDisplayName { get; set; }
}

