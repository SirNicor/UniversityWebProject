using Logger;
using Service;
using Start.Request;
namespace Start.Middleware;

public static class UseEndpoints
{
    public static void UseDiffEndpoints(this IEndpointRouteBuilder app, MyLogger logger,IConfiguration configuration)
    {
        app.AddStudentRequest(logger, 
            configuration);
        app.AddOtherRequest(logger, 
            configuration);
        app.AddAddressRequest(logger, configuration);
        app.AddPassportRequest(logger);
        app.AddAdministratorRequest(logger);
        app.AddUniversityRequest(logger);
        app.AddFacultyRequest(logger);
        app.AddDepartmentRequest(logger);
        app.AddTeacherRequest(logger);
        app.AddDisciplineRequest(logger);
        app.AddDirectionRequest(logger);
        app.AddScheduleRequest(logger);
        app.AddRegisterRequest(logger);
        app.AddAuthAndLoginRequest(configuration,logger);
        app.MapGet("/", async (HttpContext context) =>
        {
            context.Response.Headers.ContentLanguage = "ru-RU";
            context.Response.Headers.ContentType = "text/html; charset=utf-8";
            context.Response.Headers.Append("University", "system");
        }); 
    }
}