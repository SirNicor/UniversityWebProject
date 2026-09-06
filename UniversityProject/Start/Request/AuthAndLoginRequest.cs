using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Json;
using IRepositoryAll;
using Logger;
using Microsoft.IdentityModel.Tokens;
using Start.Const;
using UCore;

namespace Start.Request;

public static class AuthAndLoginRequest
{
    public static void AddAuthAndLoginRequest(this IEndpointRouteBuilder app, IConfiguration configuration,
        MyLogger logger)
    {
        app.MapPost("/Authorization", async (HttpContext ctx) =>
        {
            logger.Info("@/Authorization");
            var authRep = ctx.RequestServices.GetService<IAuthorizationRepository>();
            var user = await ctx.Request.ReadFromJsonAsync<AuthorizationDto>();
            user.BlackList = false;
            long id = await authRep.CreateAuthorizationAsync(user);
            await ctx.Response.WriteAsJsonAsync(id);
        });
        app.MapPost("/Login", async (HttpContext ctx, CancellationToken token) =>
        {
            logger.Info("@/Login");
            var authAndLoginRep = ctx.RequestServices.GetService<IAuthorizationRepository>();
            var roleRep = ctx.RequestServices.GetService<IRoleRepository>();
            using var reader = new StreamReader(ctx.Request.Body);
            var json = await reader.ReadToEndAsync(token);
            var requestData = JsonSerializer.Deserialize<JsonElement>(json);
            var auth = requestData.GetProperty("authorization").Deserialize<AuthorizationForGetJwtToken>();
            var userIdAndRole = await authAndLoginRep.GetAuthorizationsRoleForIndexAsync(auth);
            var userId = userIdAndRole.Item1;
            var rolesId =  userIdAndRole.Item2;
            if (userId is null)
            {
                return Results.Unauthorized();
            }

            bool checkPassword = await authAndLoginRep.CheckPasswordAsync(auth.Password, (long)userId);
            if (!checkPassword)
            {
                return Results.Unauthorized();
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
            return Results.Ok(new
            {
                Accessjwt = new JwtSecurityTokenHandler().WriteToken(accessJwt),
                Refreshjwt = new JwtSecurityTokenHandler().WriteToken(refreshJwt),
                Role = roles
            });
        });
        app.MapGet("/ResetAccessToken", async (HttpContext ctx) =>
        {
            logger.Info("@/ResetAccessToken");
            var request = ctx.Request;
            request.Headers.TryGetValue("authorization", out var token);
            var authAndLoginRep = ctx.RequestServices.GetService<IAuthorizationRepository>();
            var x = token.ToString();
            try
            { 
                var tokenHandler = new JwtSecurityTokenHandler();
                var claimsPrincipal = tokenHandler.ValidateToken(token, new TokenValidationParameters
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
                ctx.Response.StatusCode = 401;
                await ctx.Response.WriteAsync(MessageRequestConst.MessageUnLoginForUnauthorized);
            }
            var ver = await authAndLoginRep.CheckAndUpdateJwtTokenAsync(x);
            if (ver is null)
            {
                return Results.Unauthorized();
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
            return Results.Ok(new
            {
                Accessjwt = new JwtSecurityTokenHandler().WriteToken(accessJwt),
                Refreshjwt = new JwtSecurityTokenHandler().WriteToken(refreshJwt)
            });
        });
        app.MapGet("/CheckAccessToken", (HttpContext ctx) => Task.FromResult(Results.Ok()));
    }
}