using ServiceWorkerCronJobDemo.Services;
using Logger;
using Repository;
using UCore;

using IRepositoryAll;
namespace Start;

public class TeacherDoWorkJob : CronJobService
{

    public TeacherDoWorkJob(IScheduleConfig<TeacherDoWorkJob> config, ILogger<TeacherDoWorkJob> loggerMain, MyLogger myLogger, IServiceScopeFactory serviceScopeFactory)
        : base(config.CronExpression, config.TimeZoneInfo, loggerMain)
    {
        _loggerMain = loggerMain;
        _myLogger = myLogger;
        _serviceScopeFactory = serviceScopeFactory;
    }

    public override Task StartAsync(CancellationToken cancellationToken)
    {
        _loggerMain.LogInformation("CronJob запущен");
        _myLogger.Info("CronJob запущен", "Start:StartJob:TeacherDoWorkJob");
        return base.StartAsync(cancellationToken);
    }

    public override Task DoWork(CancellationToken cancellationToken)
    {
        _loggerMain.LogInformation($"{DateTime.Now:hh:mm:ss} Выполняется задача");
        _myLogger.Info($"{DateTime.Now:hh:mm:ss} Выполняется задача", "Start:StartJob:TeacherDoWorkJob");
        using (var scope = _serviceScopeFactory.CreateScope())
        {
            var teacherRepository = scope.ServiceProvider.GetRequiredService<IWorkerTeacherRepository>();
            _teachers = teacherRepository.ReturnList();
        }
        foreach (var teacher in _teachers)
        {
            teacher.DoWork(_myLogger);
            Thread.Sleep(120000);
        }
        return Task.CompletedTask;
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        _loggerMain.LogInformation("CronJob остановлен");
        _myLogger.Info("CronJob остановлен", "Start:StartJob:TeacherDoWorkJob");
        return base.StopAsync(cancellationToken);
    }
    
    private readonly ILogger<TeacherDoWorkJob> _loggerMain;
    private readonly MyLogger _myLogger;
    private List<Teacher> _teachers;
    private  IServiceScopeFactory _serviceScopeFactory;
}