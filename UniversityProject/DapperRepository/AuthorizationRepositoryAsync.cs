using System.Data;
using System.Data.SqlClient;
using System.Security.Cryptography;
using System.Text;
using Dapper;
using Logger;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using IRepositoryAll;

namespace Repository;
using UCore;


public class AuthorizationRepository(IGetConnectionString getConnectionString, MyLogger logger)
    : IAuthorizationRepository
{
    private readonly string _connectionString = getConnectionString.ReturnConnectionString();
    private readonly MyLogger _logger = logger;
    public async Task<AuthorizationDto> GetForLoginAuthorizationAsync(string login)
{
    await using var db = new SqlConnection(_connectionString);
    await db.OpenAsync();
    const string sqlUser = @"
            SELECT 
                Id,
                Login,
                Email,
                HashPassword AS Password,
                Salt,
                Phone,
                IdAdmin,
                IdTeacher,
                BlackList
            FROM AuthorizationTable
            WHERE Login = @login";
    var user = await db.QueryFirstOrDefaultAsync<AuthorizationDto>(sqlUser, new { login });
    if (user?.Id.HasValue == true)
    {
        const string sqlRoles = @"
                SELECT IdRole 
                FROM RoleForAuthorization 
                WHERE IdUser = @userId";
        user.Role = (await db.QueryAsync<int>(sqlRoles, new { userId = user.Id })).ToArray();
    }
    return user;
}

public async Task<AuthorizationDto> GetForIdAuthorizationAsync(long id)
{
    await using var db = new SqlConnection(_connectionString);
    await db.OpenAsync();
    const string sqlUser = @"
            SELECT 
                Id,
                Login,
                Email,
                HashPassword AS Password,
                Salt,
                Phone,
                IdAdmin,
                IdTeacher,
                BlackList
            FROM AuthorizationTable
            WHERE Id = @id";
    var user = await db.QueryFirstOrDefaultAsync<AuthorizationDto>(sqlUser, new { id });
    if (user?.Id.HasValue == true)
        {
            const string sqlRoles = @"
                SELECT IdRole 
                FROM RoleForAuthorization 
                WHERE IdUser = @userId";
            user.Role = (await db.QueryAsync<int>(sqlRoles, new { userId = user.Id })).ToArray();
        }
        
        return user;
}

public async Task<long> CreateAuthorizationAsync(AuthorizationDto dto)
{
    await using var db = new SqlConnection(_connectionString);
    await db.OpenAsync();
    await using var transaction = await db.BeginTransactionAsync();
    try
    {
        byte[] salt = RandomNumberGenerator.GetBytes(128 / 8);
        dto.Password = Convert.ToBase64String(KeyDerivation.Pbkdf2(
            password: dto.Password,
            salt: salt,
            prf: KeyDerivationPrf.HMACSHA256,
            iterationCount: 100000,
            numBytesRequested: 256 / 8));
        dto.Salt = Convert.ToBase64String(salt);
                
        const string sql = @"
                    INSERT INTO AuthorizationTable (
                        Login,
                        Email,
                        HashPassword,
                        Salt,
                        Phone,
                        IdAdmin,
                        IdTeacher,
                        BlackList
                    ) VALUES (
                        @Login,
                        @Email,
                        @Password,
                        @Salt,
                        @Phone,
                        @IdAdmin,
                        @IdTeacher,
                        @BlackList
                    );
                    SELECT CAST(SCOPE_IDENTITY() AS BIGINT);";

        var id = await db.ExecuteScalarAsync(sql, dto, transaction);
                
        if (dto.Role != null && dto.Role.Length > 0)
        {
            const string sqlRole = @"
                        INSERT INTO RoleForAuthorization (IdRole, IdUser) 
                        VALUES (@IdRole, @IdUser)";
                    
            foreach (var roleId in dto.Role)
            {
                await db.ExecuteAsync(sqlRole, new { IdRole = roleId, IdUser = id }, transaction);
            }
        }
                
        await transaction.CommitAsync();
        return (long)id;
    }
    catch (Exception ex)
    {
        _logger.Error($"Error creating authorization: {ex.Message}", "DapperRepository:AuthorizationRepositoryAsync");
        throw;
    }
}

public async Task<long> UpdateAuthorizationAsync(AuthorizationDto dto)
{
    await using  var db = new SqlConnection(_connectionString);
    await db.OpenAsync();
    await using var transaction = await db.BeginTransactionAsync();
    try
    {
        byte[] salt = RandomNumberGenerator.GetBytes(128 / 8);
        dto.Password = Convert.ToBase64String(KeyDerivation.Pbkdf2(
            password: dto.Password,
            salt: salt,
            prf: KeyDerivationPrf.HMACSHA256,
            iterationCount: 100000,
            numBytesRequested: 256 / 8));
        dto.Salt = Convert.ToBase64String(salt);
                
        const string sql = @"
                    UPDATE AuthorizationTable 
                    SET 
                        Email = @Email,
                        HashPassword = @Password,
                        Salt = @Salt,
                        Phone = @Phone,
                        IdAdmin = @IdAdmin,
                        IdTeacher = @IdTeacher,
                        BlackList = @BlackList
                    WHERE Id = @Id";

        await db.ExecuteAsync(sql, dto, transaction);
                
        if (dto.Id.HasValue)
        {
            const string deleteRoles = "DELETE FROM RoleForAuthorization WHERE IdUser = @IdUser";
            await db.ExecuteAsync(deleteRoles, new { IdUser = dto.Id }, transaction);
                    
            if (dto.Role != null & dto.Role.Length > 0)
            {
                const string insertRole = @"
                            INSERT INTO RoleForAuthorization (IdRole, IdUser) 
                            VALUES (@IdRole, @IdUser)";
                        
                foreach (var roleId in dto.Role)
                {
                    await db.ExecuteAsync(insertRole, new { IdRole = roleId, IdUser = dto.Id }, transaction);
                }
            }
        }
                
        await transaction.CommitAsync();
        return dto.Id ?? 0;
    }
    catch (Exception ex)
    {
        _logger.Error($"Error updating authorization: {ex.Message}", "DapperRepository:AuthorizationRepositoryAsync");
        throw;
    }
}

public async Task<(long?, int[]?)> GetAuthorizationsRoleForIndexAsync(AuthorizationForGetJwtToken dto)
{
    await using var db =  new SqlConnection(_connectionString);
    await db.OpenAsync();
    string sql = @"
            SELECT rfa.IdRole
            FROM AuthorizationTable at
            INNER JOIN RoleForAuthorization rfa ON at.Id = rfa.IdUser
            WHERE at.Login = @login AND
                  at.Email = @email AND
                  at.Phone = @phone";
    var roles = (await db.QueryAsync<int>(sql, dto)).ToArray();
    roles = roles.Length > 0 ? roles : null;
    sql = @"
                SELECT Id
                FROM AuthorizationTable
                WHERE Login = @login AND
                      Email = @email AND
                      Phone = @phone";
    var id = await db.QueryFirstOrDefaultAsync<long>(sql, dto);
    return (id, roles);
    
}

public async Task<string[]> GetAllRolesAsync(int[] idRoles)
{
    await using var db =  new SqlConnection(_connectionString);
    await db.OpenAsync();
    var sql = @"
            SELECT Name
            FROM Role 
            WHERE Id IN @idRoles";
    return (await db.QueryAsync<string>(sql, new{idRoles})).ToArray();
    
}

public async Task<bool> CheckPasswordAsync(string password, long id)
    {
        await using var db =  new SqlConnection(_connectionString);
        await db.OpenAsync();
        const string sql = @"
                SELECT HashPassword AS Password, Salt
                FROM AuthorizationTable
                WHERE Id = @id";
        PasswordAndSalt? passwordAndSalt = await db.QueryFirstOrDefaultAsync<PasswordAndSalt>(sql, new { id });
        if (passwordAndSalt is null)
        {
            return false;
        }
        string hashedPassword = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: password,
                salt: Convert.FromBase64String(passwordAndSalt.Salt),
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: 100000,
                numBytesRequested: 256 / 8));
        return hashedPassword == passwordAndSalt.Password;
    }

    public async Task<RefreshJWTTokenDTO> GetJwtTokenAsync(string token)
    {
        await using var db =  new SqlConnection(_connectionString);
        await db.OpenAsync();
        try
        {
            using var sha256 = SHA256.Create();
            byte[] hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(token));
            string tokenHash = Convert.ToBase64String(hashBytes);
            const string sql = @"
                    SELECT Id, Token, RevokedAt, IdAuthorizationTable 
                    FROM RefreshJWTToken 
                    WHERE Token = @token";
            var jwtToken = await db.QueryFirstOrDefaultAsync<RefreshJWTTokenDTO>(sql, new { token = tokenHash });
            return jwtToken ?? new RefreshJWTTokenDTO();
        }
        catch (Exception ex)
        {
            _logger.Error($"Error getting token by signature {token}: {ex.Message}", "DapperRepository:AuthorizationRepositoryAsync");
            throw;
        }
    }

    public async Task<long?> CheckAndUpdateJwtTokenAsync(string token)
    {
        await using var db =  new SqlConnection(_connectionString);
        await db.OpenAsync();
        await using var transaction = await db.BeginTransactionAsync();
        try
        {
            using var sha256 = SHA256.Create();
            byte[] hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(token));
            string tokenHash = Convert.ToBase64String(hashBytes);
            const string selectSql = @"
                        SELECT Id, Token, RevokedAt, IdAuthorizationTable 
                        FROM RefreshJWTToken 
                        WHERE Token = @token";
            var existingToken = await db.QueryFirstOrDefaultAsync<RefreshJWTTokenDTO>(
                selectSql,
                new { token = tokenHash },
                transaction
            );
                    
            if (existingToken != null & !existingToken.RevokedAt)
            {
                const string updateSql = @"
                            UPDATE RefreshJWTToken 
                            SET RevokedAt = 1 
                            WHERE Id = @Id";
                await db.ExecuteAsync(updateSql, new { existingToken.Id }, transaction);
                await transaction.CommitAsync();
                return existingToken.IdAuthorizationTable;
            }
                    
            await transaction.CommitAsync();
            return null;
        }
        catch (Exception ex)
        {
            _logger.Error($"Error checking and updating token: {ex.Message}", "DapperRepository:AuthorizationRepositoryAsync");
            throw;
        }
    }

    public async Task<long> CreateJwtTokenAsync(RefreshJWTTokenDTO dto)
    {
        await using var db =  new SqlConnection(_connectionString);
        await db.OpenAsync();
        try
        {
            using var sha256 = SHA256.Create();
            byte[] hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(dto.Token));
            string tokenHash = Convert.ToBase64String(hashBytes);
            const string sql = @"
                    INSERT INTO RefreshJWTToken (Token, IdAuthorizationTable, RevokedAt) 
                    VALUES (@Token, @IdAuthorizationTable, 0);
                    SELECT SCOPE_IDENTITY();";
            var id = await db.QuerySingleAsync<long>(sql, new { Token = tokenHash, dto.IdAuthorizationTable });
            return id;
        }
        catch (Exception ex)
        {
            _logger.Error($"Error creating token: {ex.Message}", "DapperRepository:AuthorizationRepositoryAsync");
            throw;
        }
    }

    public async Task DeleteJwtTokenAsync(string token)
    {
        await using var db =  new SqlConnection(_connectionString);
        await db.OpenAsync();
        try
        {
            using var sha256 = SHA256.Create();
            byte[] hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(token));
            string tokenHash = Convert.ToBase64String(hashBytes);
            const string sql = "DELETE FROM RefreshJWTToken WHERE Token = @token";
            var affectedRows = await db.ExecuteAsync(sql, new { token = tokenHash });
            if (affectedRows == 0)
            {
                _logger.Warning($"No token found with signature: {token}", "DapperRepository:AuthorizationRepositoryAsync");
            }
        }
        catch (Exception ex)
        {
            _logger.Error($"Error deleting token: {ex.Message}", "DapperRepository:AuthorizationRepositoryAsync");
            throw;
        }
        
    }
}