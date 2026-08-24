using ServiceWorkerCronJobDemo.Services;

namespace Start;

static class DoCronJob
{
    public static void MakeCronJob(this IServiceCollection services, IConfiguration configuration)
    {
        var timeSalaryJob = configuration.GetValue<string>("StartJobTime:SalaryJob");
        var timeTeacherDoSession =  configuration.GetValue<string>("StartJobTime:TeacherDoSession");
        var timeTeacherDoWork =  configuration.GetValue<string>("StartJobTime:TeacherDoWork");
        services.AddCronJob<TeacherDoWorkJob>(c =>
        {
            c.TimeZoneInfo = TimeZoneInfo.Local;
            c.CronExpression = @timeTeacherDoWork;
        });
        services.AddCronJob<TeacherDoSessionJob>(c =>
        {
            c.TimeZoneInfo = TimeZoneInfo.Local;
            c.CronExpression = @timeTeacherDoSession;
        });
        services.AddCronJob<SalaryJob>(c =>
        {
            c.TimeZoneInfo = TimeZoneInfo.Local;
            c.CronExpression = @timeSalaryJob;
        });
    }
}   