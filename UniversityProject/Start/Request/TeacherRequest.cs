using IRepositoryAll;
using Logger;
using UCore;

namespace Start.Request;

static class TeacherRequest
{
    public static void AddTeacherRequest(this IEndpointRouteBuilder app, MyLogger logger)
    {
        app.MapGet("/Teacher/{id}", async (int id, HttpContext context) =>
        {
            var service = context.RequestServices.GetService<IWorkerTeacherRepository>();   
            var teacher = service.GetForId(id);
            await context.Response.WriteAsJsonAsync(teacher); 
        });
        app.MapGet("/Teacher", async context =>
        {
            var service = context.RequestServices.GetService<IWorkerTeacherRepository>();
            var teachers = service.ReturnList();            
            await context.Response.WriteAsJsonAsync(teachers);
        });
        app.MapPost("/Teacher", async context =>
        {
            var request = context.Request;
            var service = context.RequestServices.GetService<IWorkerTeacherRepository>();
            Teacher teacher = await request.ReadFromJsonAsync<Teacher>();
            var id = service.Create(teacher);
            await context.Response.WriteAsJsonAsync(id);
        });
        app.MapPut("/Teacher/{id}", async (long id, HttpContext context) =>
        {
            var request = context.Request;
            var service =  context.RequestServices.GetService<IWorkerTeacherRepository>();
            Teacher teacher = await request.ReadFromJsonAsync<Teacher>();
            teacher.TeacherId = id;
            id = service.Update(teacher);
            await context.Response.WriteAsJsonAsync(id);
        });
        app.MapDelete("/Teacher/{id}", async (long id, HttpContext context) =>
        {
            var service = context.RequestServices.GetService<IWorkerTeacherRepository>();
            service.Delete(id);
        });
    }
}