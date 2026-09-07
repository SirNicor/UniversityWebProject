using ServiceWorkerCronJobDemo.Services;
using Logger;
using Repository;
using UCore;
using UJob;
namespace Start;

public class SalaryJob(
    IScheduleConfig<TeacherDoWorkJob> config,
    ILogger<TeacherDoWorkJob> loggerMain,
    MyLogger myLogger,
    IServiceScopeFactory salaryJob)
    : CronJobService(config.CronExpression, config.TimeZoneInfo, loggerMain)
{
    public override Task StartAsync(CancellationToken cancellationToken)
    {
        loggerMain.LogInformation("CronJob запущен");
        myLogger.Info("CronJob запущен", "Start:StartJob:SalaryJob");
        return base.StartAsync(cancellationToken);
    }

    public override Task DoWork(CancellationToken cancellationToken)
    {
        loggerMain.LogInformation($"{DateTime.Now:hh:mm:ss} Выполняется задача");
        myLogger.Info($"{DateTime.Now:hh:mm:ss} Выполняется задача", "Start:StartJob:SalaryJob");
        using (var scope = salaryJob.CreateScope())
        {
            var salaryJob =  scope.ServiceProvider.GetRequiredService<ISalaryJob>();
            salaryJob.DoWorkAsync();
        }
        return Task.CompletedTask;
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        loggerMain.LogInformation("CronJob остановлен");
        myLogger.Info("CronJob остановлен", "Start:StartJob:SalaryJob");
        return base.StopAsync(cancellationToken);
    }
}