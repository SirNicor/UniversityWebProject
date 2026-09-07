using System.Reflection;
using Logger;

namespace Start.Middleware;

public class ExceptionHandlerMiddleware(RequestDelegate next, MyLogger logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (OperationCanceledException ex)
        {
        }
        catch (TargetInvocationException ex)
        {
        }
        catch (Exception ex)
        {
            logger.Error(ex.Message + " " + ex.StackTrace + Environment.NewLine + "Source =" + ex.Source,"ExceptionHandlerMiddleware");
            if (!context.Response.HasStarted)
            {
                context.Response.Clear();
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                await context.Response.WriteAsync(ex.Message);
            }
        }
    }
}