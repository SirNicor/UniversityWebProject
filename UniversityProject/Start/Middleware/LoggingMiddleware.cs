using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;

namespace Start.Middleware;
using Logger;

public class LoggingMiddleware(RequestDelegate next, MyLogger logger, IConfiguration configuration)
{

    public async Task InvokeAsync(HttpContext context)
    {
        var request = context.Request;
        logger.Info($"Request baseUrl = {request.Host}, path = {request.Path}," +
                     $"contentType : {request.ContentType}, method =  {request.Method}", "LoggingMiddleware");
        await next(context);
        logger.Info($"Request: path = {request.Path}, contentType : {context.Response.ContentType}", "LoggingMiddleware");
    }
}