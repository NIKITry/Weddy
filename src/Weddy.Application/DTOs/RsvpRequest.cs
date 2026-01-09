using System.ComponentModel.DataAnnotations;
using Weddy.Domain.Entities;

namespace Weddy.Application.DTOs;

public class RsvpRequest
{
    [Required]
    public InvitationStatus Status { get; set; }
    
    [MaxLength(5000)]
    public string? Note { get; set; }
}

