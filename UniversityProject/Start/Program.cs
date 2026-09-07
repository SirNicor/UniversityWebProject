using System.Security.Claims;
using Logger;
using UJob;
using Start;
using Repository.Migrations;
using ApiTelegramBot;
using Dapper;
using Repository;
using Start.Const;
using Start.Middleware;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddJsonFile("appsettings.json");
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowVueLocalhost", policy =>
    {
        policy
            .WithOrigins("http://localhost:5173", "https://localhost:3000")
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });
});
IConfiguration appConfig = builder.Configuration;
ConfigurationLogger cl = new ConfigurationLogger(appConfig);
MyLogger logger = cl.Get();
try
{
    SqlMapper.AddTypeHandler(new TimeHandler());
    StartMigrations startMigrations = new StartMigrations(appConfig, logger);
    startMigrations.Start();
    builder.Services.AddInfrastructureServices(logger, appConfig);
    builder.Services.MakeCronJob(appConfig);
    builder.Services.CreateJwtTokens(appConfig);
    builder.Services.AddAuthorization(options =>
    {
        options.AddPolicy("AdministrationAdministrator", policy => policy.RequireClaim(ClaimTypes.Role,AuthorizationPolicyConst.AdministratorAdmin));
        options.AddPolicy("Teacher", policy => policy.RequireClaim(ClaimTypes.Role,AuthorizationPolicyConst.Teacher));
        options.AddPolicy("StudentAdministrator", policy => policy.RequireClaim(ClaimTypes.Role,AuthorizationPolicyConst.StudentAdministrator));
    });
    var app = builder.Build();
    app.UseMiniProfiler();
    app.UseMiniProfilerLog();
    app.UseDiffEndpoints(logger, appConfig);
    app.UseCors(builder => builder.AllowAnyOrigin()
        .AllowAnyHeader()
        .AllowAnyMethod());
    app.UseLoggingMiddleware();
    app.UseExceptionHandlerMiddleware();
    app.UseAuthenticationMiddleware();
    app.UseAuthorization();
    Thread botThread = new Thread(()=>
    {
        using var botScope = app.Services.CreateScope();
        var bot = botScope.ServiceProvider.GetRequiredService<IStartBot>();
        _ = Task.Run(async () => await bot.ListenForMessagesAsync());
    });
    botThread.Start();
    app.MapGet("/Worker", () => app.Services.GetService<IPrintWorkersJob>().DoWorkAsync());
    app.MapGet("/ScoresOfStudents", () => app.Services.GetService<IScoresOfStudentsJob>().DoWorkAsync());
    app.MapGet("/InfoCouplesAttendance", () => app.Services.GetService<IInfoCouplesAttendanceJob>().DoWorkAsync());
    app.Run();
}
catch (Exception ex)
{
    logger.Error("Error in general trycatch " + ex.Message + ex.Source + ex.StackTrace + ex.TargetSite, "General try catch: ");
}
    