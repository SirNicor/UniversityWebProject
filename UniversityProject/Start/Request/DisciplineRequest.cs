using IRepositoryAll;
using Logger;
using UCore;

namespace Start.Request;

static class DisciplineRequest
{
    public static void AddDisciplineRequest(this IEndpointRouteBuilder app, MyLogger logger)
    {
        app.MapGet("/Discipline/{id}", async (int id, HttpContext context) =>
        {
            var service = context.RequestServices.GetService<IDisciplineRepository>();   
            var discipline = service.GetForId(id);
            await context.Response.WriteAsJsonAsync(discipline); 
        });
        app.MapGet("/Discipline", async context =>
        {
            var service = context.RequestServices.GetService<IDisciplineRepository>();
            var disciplines = service.ReturnList();            
            await context.Response.WriteAsJsonAsync(disciplines);
        });
        app.MapPut("/Discipline/{id}", async (long id, HttpContext context) =>
        {
            var request = context.Request;
            var service =  context.RequestServices.GetService<IDisciplineRepository>();
            DisciplineDto discipline = await request.ReadFromJsonAsync<DisciplineDto>();
            discipline.DisciplineId = id;
            id = service.Update(discipline);
            await context.Response.WriteAsJsonAsync(id);
        });
        app.MapPost("/Discipline", async context =>
        {
            var request = context.Request;
            var service =  context.RequestServices.GetService<IDisciplineRepository>();
            DisciplineDto discipline = await request.ReadFromJsonAsync<DisciplineDto>();
            var id = service.Create(discipline);
            await context.Response.WriteAsJsonAsync(id);
        });
        app.MapDelete("/Discipline/{id}", (long id, HttpContext context) =>
        {
            var service = context.RequestServices.GetService<IDisciplineRepository>();
            service.Delete(id);
            return Task.CompletedTask;
        });
    }
}