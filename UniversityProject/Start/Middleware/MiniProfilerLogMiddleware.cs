using System.Text.Json;
using Logger;
using StackExchange.Profiling;
using StackExchange.Profiling.Internal;

namespace Start.Middleware;

public class MiniProfilerLogMiddleware
{
    private readonly RequestDelegate _next;
    private readonly MyLogger _logger;

    public MiniProfilerLogMiddleware(RequestDelegate next, MyLogger logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        await _next(context);
        var profiler = MiniProfiler.Current;
        if (profiler != null)
        {
            profiler.Stop();
            if (profiler.Root != null)
            {
                _logger.Info("Запрос {Path} выполнен за {Duration} мс. Всего SQL запросов: {SqlCount}","MiniProfilerLog minimum Info:");
                string fullLogJson = profiler.ToJson();
                using JsonDocument doc = JsonDocument.Parse(fullLogJson);
                var options = new JsonSerializerOptions { WriteIndented = true };
                string prettyJson = JsonSerializer.Serialize(doc.RootElement, options);
                _logger.Info("MiniProfiler info: Детали MiniProfiler: {CustomProfileData}" + prettyJson,"MiniProfilerLog detail Info:");
            }
        }
    }
}   