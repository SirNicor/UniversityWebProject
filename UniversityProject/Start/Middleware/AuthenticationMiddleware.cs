using System.IdentityModel.Tokens.Jwt;
using Logger;
using Microsoft.IdentityModel.Tokens;
using Start.Const;

namespace Start.Middleware;

public class AuthenticationMiddleware(RequestDelegate next, IConfiguration configuration, MyLogger logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var path = context.Request.Path.ToString();
        if (path.ToLower() != "/authorization" && path.ToLower() != "/login" && path.ToLower() != "/resetaccesstoken" && path != "/")
        {
            context.Request.Headers.TryGetValue("authorization", out var token);
            if (token.ToString() == null)
            {
                await SendBadRequest(context, 401, MessageRequestConst.MessageUnLoginForUnauthorized);
                return;
            }

            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Convert.FromBase64String(configuration["Auth:Key"]);
                var claimsPrincipal = tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = true,
                    ValidAudience = configuration["Auth:AUDIENCE"],
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);
                context.User = claimsPrincipal;
                foreach (var x in claimsPrincipal.Claims)
                {
                    logger.Info($"Claims - {x.Type};  {x.Value}");
                }
            }
            catch (SecurityTokenExpiredException)
            {
                await SendBadRequest(context, 401, MessageRequestConst.MessageSendRefreshTokenForUnauthorized);
                return;
            }
            catch (SecurityTokenInvalidSignatureException)
            {
                await SendBadRequest(context, 401, MessageRequestConst.MessageUnLoginForUnauthorized);
                return;
            }
            catch (Exception ex)
            {
                await SendBadRequest(context, 400, MessageRequestConst.MessageForBadRequest);
                return;
            }
        }
        await next(context);
    }

    public static async Task SendBadRequest(HttpContext context, int statusCode, string message)
    {
        if (context.Response.HasStarted)
        {
            return;
        }
        context.Response.Clear();
        context.Response.StatusCode = statusCode;
        await context.Response.WriteAsync(message);
    }
}