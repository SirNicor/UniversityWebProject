using System.Text.Json;
using IRepositoryAll;
using Logger;
using UCore;

namespace Start.Request;

static class StudentRequest
{
    public static void AddStudentRequest(this IEndpointRouteBuilder app, MyLogger logger, IConfiguration configuration)
    {
        app.MapGet("/Student/{studentId}", async (long studentId, CancellationToken token, HttpContext context) =>
        {   
            var service = context.RequestServices.GetService<IStudentRepository>();   
            var student = await service.GetStudentPageAsync(studentId, token);
            return Results.Json(student, statusCode: 200);
        }).RequireAuthorization("Teacher");
        app.MapGet("/Student/Page/{count}",async (int count, CancellationToken token, HttpContext context) =>
        {
            var service = context.RequestServices.GetService<IStudentRepository>();
            var allCount = await service.GetCountAsync(token);
            var countOfPage = allCount / count +  (allCount % count == 0? 0: 1);
            logger.Info($"student/Page/{count} = > {countOfPage}", "StudentRequest");
            return Results.Json(countOfPage, statusCode: 200);
        }).RequireAuthorization("Teacher");
        app.MapGet("/Student", async(string? filter, string? sortKey, string? sortOrder, int page, int count, CancellationToken token,  HttpContext context) =>
        {
            FilterDto filterDto = JsonSerializer.Deserialize<FilterDto>(filter);
            int firstId = (page-1) * count;
            var service = context.RequestServices.GetService<IStudentRepository>();
            var studentAndPage = await service.GetStudentTableDto(firstId, count, sortKey,
                sortOrder, filterDto, token);
            var allCount = studentAndPage.Item2;
            var countOfPage = allCount / count +  (allCount % count == 0? 0: 1);
            logger.Info($"student/{page} {count} {firstId} {sortKey} {sortOrder}", "StudentRequest");
            return Results.Json(new Tuple<List<StudentTableDTO>, long>(studentAndPage.Item1, countOfPage), statusCode: 200);
        }).RequireAuthorization("Teacher");
        app.MapPost("/Student", async (CancellationToken token, HttpContext context) =>
        {
            var request = context.Request;
            var options = new JsonSerializerOptions();
            options.Converters.Add(new ConfiguratonJsonDateOnly());
            var service =  context.RequestServices.GetService<IStudentRepository>();
            var student = await request.ReadFromJsonAsync<StudentDtoForPage>(options, token);
            var id = await service.CreateAsync(student, token);
            return Results.Json(id, statusCode: 200);
        }).RequireAuthorization("StudentAdministrator");
        app.MapPut("/Student/{id}", async (long id, CancellationToken token, HttpContext context) =>
        {
            var request = context.Request;
            var service = context.RequestServices.GetService<IStudentRepository>();
            StudentDtoForPage student = await request.ReadFromJsonAsync<StudentDtoForPage>();
            student.studentId = id;
            var idUpdate = await service.UpdateAsync(student, token);
            return Results.Json(idUpdate, statusCode: 200);
        }).RequireAuthorization("StudentAdministrator");
        app.MapDelete("/Student/{id}", async (long id, CancellationToken token,  HttpContext context) =>
        {
            var service = context.RequestServices.GetService<IStudentRepository>();
            await service.DeleteAsync(id, token);
            return Results.Ok();
        }).RequireAuthorization("StudentAdministrator");
    }    
}