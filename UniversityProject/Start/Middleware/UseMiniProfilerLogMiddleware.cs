namespace Start.Middleware;

public static class UseMiniProfilerLogMiddleware
{
    public static IApplicationBuilder UseMiniProfilerLog(this IApplicationBuilder  app)
    {
        return app.UseMiddleware<MiniProfilerLogMiddleware>();
    }
}