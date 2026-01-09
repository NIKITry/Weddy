using System.ComponentModel.DataAnnotations;

namespace Weddy.Application.DTOs;

public class UpdateInvitationRequest
{
    [MaxLength(500)]
    public string? DisplayName { get; set; }
    
    public string? InvitationText { get; set; }
    
    [MaxLength(1000)]
    public string? MetaNote { get; set; }
    
    public bool? Archived { get; set; }
}

