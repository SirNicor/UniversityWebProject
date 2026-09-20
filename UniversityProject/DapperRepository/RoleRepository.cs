using System.Data;
using System.Data.SqlClient;
using Dapper;
using Logger;
using UCore;

using IRepositoryAll;
namespace Repository;

public class RoleRepository(IGetConnectionString getConnectionString, MyLogger logger) : IRoleRepository
{
    private readonly string _connectionString = getConnectionString.ReturnConnectionString();

    public RoleAccessDto GetRoleAccess(int roleId)
    {
        using IDbConnection db = new SqlConnection(_connectionString);
        const string sql = @"SELECT Id, NameRole, TypeOperation, AccessPage
        FROM view_role";

        try
        {
            var rawResults = db.Query<RoleAccessRaw>(sql + "WHERE Id = @roleId", new { roleId }).ToList();
            
            if (rawResults.Count == 0)
                return null;

            var permissions = rawResults
                .Select(x => Tuple.Create(x.TypeOperation, x.AccessPage))
                .Distinct()
                .ToArray();

            return new RoleAccessDto
            {
                Id = rawResults[0].Id,
                NameRole = rawResults[0].NameRole,
                TypeOperationAccessPage = permissions
            };
        }
        catch (Exception ex)
        {
            logger.Error($"Error getting role access for RoleId {roleId}: {ex.Message}", "DapperRepository:RoleRepository");
            throw;
        }
    }

    public RoleAccessDto[] GetRoleAccess(int[] rolesId)
    {
        if (rolesId == null || rolesId.Length == 0)
            return Array.Empty<RoleAccessDto>();

        using IDbConnection db = new SqlConnection(_connectionString);
        const string sql = @"
            SELECT Id, NameRole, TypeOperation, AccessPage
            FROM view_role
            WHERE Id IN @rolesId";

        try
        {
            var rawResults = db.Query<RoleAccessRaw>(sql, new { rolesId }).ToList();
            
            if (rawResults.Count == 0)
                return [];

            var result = rawResults
                .GroupBy(x => new { x.Id, x.NameRole })
                .Select(g => new RoleAccessDto
                {
                    Id = g.Key.Id,
                    NameRole = g.Key.NameRole,
                    TypeOperationAccessPage = g
                        .Select(x => Tuple.Create(x.TypeOperation, x.AccessPage))
                        .Distinct()
                        .ToArray()
                })
                .ToArray();

            return result;
        }
        catch (Exception ex)
        {
            logger.Error($"Error getting role access for roles [{string.Join(",", rolesId)}]: {ex.Message}", "DapperRepository:RoleRepository");
            throw;
        }
    }
}