using Dapper;
using Npgsql;
using Weddy.Domain.Entities;

namespace Weddy.Infrastructure.Database;

public static class DapperConfig
{
    public static void Configure()
    {
        // Регистрация маппера для enum InvitationStatus
        SqlMapper.AddTypeHandler(new InvitationStatusTypeHandler());
    }
}

public class InvitationStatusTypeHandler : Dapper.SqlMapper.TypeHandler<InvitationStatus>
{
    public override void SetValue(System.Data.IDbDataParameter parameter, InvitationStatus value)
    {
        parameter.Value = value switch
        {
            InvitationStatus.None => "none",
            InvitationStatus.Yes => "yes",
            InvitationStatus.No => "no",
            InvitationStatus.Maybe => "maybe",
            _ => "none"
        };
    }

    public override InvitationStatus Parse(object value)
    {
        if (value == null || value == DBNull.Value)
            return InvitationStatus.None;

        return value.ToString()?.ToLower() switch
        {
            "none" => InvitationStatus.None,
            "yes" => InvitationStatus.Yes,
            "no" => InvitationStatus.No,
            "maybe" => InvitationStatus.Maybe,
            _ => InvitationStatus.None
        };
    }
}

