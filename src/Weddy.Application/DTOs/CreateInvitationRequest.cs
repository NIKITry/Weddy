using System.ComponentModel.DataAnnotations;

namespace Weddy.Application.DTOs;

public class CreateInvitationRequest
{
    [Required]
    [MaxLength(500)]
    public string DisplayName { get; set; } = string.Empty;
    
    public string? InvitationText { get; set; }
    
    [MaxLength(1000)]
    public string? MetaNote { get; set; }
}

