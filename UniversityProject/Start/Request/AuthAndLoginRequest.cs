using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Json;
using IRepositoryAll;
using Logger;
using Microsoft.IdentityModel.Tokens;
using Service;
using Start.Const;
using Telegram.Bot.Types;
using UCore;

namespace Start.Request;

public static class AuthAndLoginRequest
{
    public static void AddAuthAndLoginRequest(this IEndpointRouteBuilder app, IConfiguration configuration,
        MyLogger logger)
    {
        app.MapPost("/Authorization", async (HttpContext ctx) =>
        {
            logger.Info("@/Authorization", "AddAuthAndLoginRequest");
            var authRep = ctx.RequestServices.GetService<IAuthorizationRepository>();
            var user = await ctx.Request.ReadFromJsonAsync<AuthorizationDto>();
            user.BlackList = false;
            long id = await authRep.CreateAuthorizationAsync(user);
            await ctx.Response.WriteAsJsonAsync(id);
        });
        app.MapPost("/Login", async (HttpContext ctx, CancellationToken token) =>
        {
            logger.Info("@/Login", "AddAuthAndLoginRequest");
            var loginGroupService = ctx.RequestServices.GetService<ILoginGroupService>();
            using var reader = new StreamReader(ctx.Request.Body);
            var json = await reader.ReadToEndAsync(token);
            var requestData = JsonSerializer.Deserialize<JsonElement>(json);
            var auth = requestData.GetProperty("authorization").Deserialize<AuthorizationForGetJwtToken>();
            var result = await loginGroupService.AsyncLogin(auth, token);
            if (result == null)
            {
                return Results.Unauthorized();
            }
            return Results.Ok(new
            {
                Accessjwt = new JwtSecurityTokenHandler().WriteToken(result.AccessToken),
                Refreshjwt = new JwtSecurityTokenHandler().WriteToken(result.RefreshToken),
                Role = result.Roles
            });
        });
        app.MapGet("/ResetAccessToken", async (HttpContext ctx) =>
        {
            logger.Info("@/ResetAccessToken", "AddAuthAndLoginRequest");
            var request = ctx.Request;
            request.Headers.TryGetValue("authorization", out var token);
            var loginGroupService = ctx.RequestServices.GetService<ILoginGroupService>();
            var x = token.ToString();
            var result = await loginGroupService.AsyncResetAccessToken(x);
            if (result == null)
            {
                return Results.Unauthorized();
            }

            if (result.Message != null)
            {
                var mess = (typeof(MessageRequestConst)).GetProperty(result.Message).GetValue(null).ToString();
                ctx.Response.ContentType = "application/json";
                if (result.HttpCode == null)
                {
                    ctx.Response.StatusCode = (int)result.HttpCode;
                }
                else
                {
                    ctx.Response.StatusCode = 401;
                }
                await ctx.Response.WriteAsync(mess);
            }
            return Results.Ok(new
            {
                Accessjwt = new JwtSecurityTokenHandler().WriteToken(result.AccessToken),
                Refreshjwt = new JwtSecurityTokenHandler().WriteToken(result.RefreshToken)
            });
        });
        app.MapGet("/CheckAccessToken", (HttpContext ctx) => Task.FromResult(Results.Ok()));
    }
}   