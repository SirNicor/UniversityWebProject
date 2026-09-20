using IRepositoryAll;
using Logger;

namespace Start.Request;

static class DirectionRequest
{
    public static void AddDirectionRequest(this IEndpointRouteBuilder app, MyLogger logger)
    {
        app.MapGet("/Direction/{id}", async (int id, HttpContext context) =>
        {
            var service = context.RequestServices.GetService<IDirectionRepository>();   
            var department = service.GetForId(id);
            await context.Response.WriteAsJsonAsync(department); 
        });
        app.MapGet("/Direction", async context =>
        {
            var service = context.RequestServices.GetService<IDirectionRepository>();
            var departments = service.ReturnList();            
            await context.Response.WriteAsJsonAsync(departments);
        });
    }
}