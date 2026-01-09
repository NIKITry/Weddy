namespace Weddy.Application.Services;

public interface ITokenGenerator
{
    string GenerateToken(string? coupleNames = null);
}

