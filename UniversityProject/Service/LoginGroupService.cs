using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Json;
using IRepositoryAll;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using UCore;
using UCore.DTO;

namespace Service;

public class LoginGroupService(IAuthorizationRepository authAndLoginRep, IRoleRepository roleRep, IConfiguration configuration) : ILoginGroupService
{
    public async Task<JWTTokenDto?> AsyncLogin(AuthorizationForGetJwtToken auth, CancellationToken token)
    {
        var userIdAndRole = await authAndLoginRep.GetAuthorizationsRoleForIndexAsync(auth);
        var userId = userIdAndRole.Item1;
        var rolesId =  userIdAndRole.Item2;
        if (userId is null)
        {
            return null;
        }

        bool checkPassword = await authAndLoginRep.CheckPasswordAsync(auth.Password, (long)userId);
        if (!checkPassword)
        {
            return null;
        }

        var roles = roleRep.GetRoleAccess((int[])rolesId);
        string[] nameRoles = new string[roles.Length];
        for (int i = 0; i < roles.Length; i++)
        {
            nameRoles[i] = roles[i].NameRole;
        }
        var jwtPayload = new JwtPayload()
        {
        {"exp", DateTimeOffset.UtcNow.AddMinutes(Convert.ToInt64(configuration.GetSection("Auth:TimeAccessJwtToken").Value)).ToUnixTimeSeconds()},
        {"aud", configuration.GetSection("Auth:Audience").Value},
        { ClaimTypes.Name, auth.Login},
        { ClaimTypes.Email, auth.Email},
        {ClaimTypes.MobilePhone, auth.Phone},
        { ClaimTypes.Role, nameRoles.ToList()},
        };
        var key = new SymmetricSecurityKey(Convert.FromBase64String(configuration["Auth:Key"]));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var header = new JwtHeader(creds);
        var accessJwt = new JwtSecurityToken(
        header: header,
        payload: jwtPayload);
        jwtPayload = new JwtPayload(){
        { ClaimTypes.NameIdentifier, auth.Id.ToString() },
        { "exp", DateTimeOffset.UtcNow.AddMinutes(Convert.ToInt64(configuration.GetSection("Auth:TimeRefreshJwtToken").Value)).ToUnixTimeSeconds()},
        {"aud", configuration.GetSection("Auth:Audience").Value},       
        { "token_type", "refresh" } 
        };
        var refreshJwt = new JwtSecurityToken(
        header: header,
        payload: jwtPayload);
        RefreshJWTTokenDTO refreshJwtDto = new RefreshJWTTokenDTO
        {
        Token = new JwtSecurityTokenHandler().WriteToken(refreshJwt),
        RevokedAt = false,
        IdAuthorizationTable = (long)userId
        };
        await authAndLoginRep.CreateJwtTokenAsync(refreshJwtDto);
        return new JWTTokenDto()
        {
            AccessToken = accessJwt,
            RefreshToken = refreshJwt,
            Roles = nameRoles
        };
    }

    public async Task<JWTTokenDto> AsyncResetAccessToken(string jwt)
    {
            try
            { 
                var tokenHandler = new JwtSecurityTokenHandler();
                var claimsPrincipal = tokenHandler.ValidateToken(jwt, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Convert.FromBase64String(configuration["Auth:Key"])),
                    ValidateIssuer = false,
                    ValidateAudience = true,
                    ValidAudience = configuration["Auth:AUDIENCE"],
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);
            }
            catch
            {
                return new JWTTokenDto()
                {
                    AccessToken = null,
                    RefreshToken = null,
                    Message = "MessageUnLoginForUnauthorized",
                    HttpCode = 401
                };
            }
            var ver = await authAndLoginRep.CheckAndUpdateJwtTokenAsync(jwt);
            if (ver is null)
            {
                return null;
            }

            var auth = await authAndLoginRep.GetForIdAuthorizationAsync((long)ver);
            var roles = await authAndLoginRep.GetAllRolesAsync(auth.Role);
            var jwtPayload = new JwtPayload()
            {
                {"exp", DateTimeOffset.UtcNow.AddMinutes(Convert.ToInt64(configuration.GetSection("Auth:TimeAccessJwtToken").Value)).ToUnixTimeSeconds()},
                {"aud", configuration.GetSection("Auth:Audience").Value},
                { ClaimTypes.Name, auth.Login},
                { ClaimTypes.Email, auth.Email},
                {ClaimTypes.MobilePhone, auth.Phone},
                { ClaimTypes.Role, roles.ToList()},
            };
            var key = new SymmetricSecurityKey(Convert.FromBase64String(configuration["Auth:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var header = new JwtHeader(creds);
            var accessJwt = new JwtSecurityToken(
                header: header,
                payload: jwtPayload);
            jwtPayload = new JwtPayload(){
                { ClaimTypes.NameIdentifier, auth.Id.ToString() },
                { "exp", DateTimeOffset.UtcNow.AddMinutes(Convert.ToInt64(configuration.GetSection("Auth:TimeRefreshJwtToken").Value)).ToUnixTimeSeconds()},
                {"aud", configuration.GetSection("Auth:Audience").Value},
                { "token_type", "refresh" } 
            };
            var refreshJwt = new JwtSecurityToken(
                header: header,
                payload: jwtPayload);
            RefreshJWTTokenDTO refreshJwtDto = new RefreshJWTTokenDTO
            {
                Token = new JwtSecurityTokenHandler().WriteToken(refreshJwt),
                RevokedAt = false,
                IdAuthorizationTable = (long)auth.Id
            };
            await authAndLoginRep.CreateJwtTokenAsync(refreshJwtDto);
            return new JWTTokenDto()
            {
                AccessToken = accessJwt,
                RefreshToken = refreshJwt
            };
    }
}