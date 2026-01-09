using Weddy.Domain.Entities;

namespace Weddy.Application.DTOs;

public class InvitationListResponse
{
    public List<InvitationDto> Items { get; set; } = new();
    public int Total { get; set; }
}

