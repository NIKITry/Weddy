using Npgsql;

namespace Weddy.Application.Database;

public interface IDbConnectionFactory
{
    NpgsqlConnection CreateConnection();
}

