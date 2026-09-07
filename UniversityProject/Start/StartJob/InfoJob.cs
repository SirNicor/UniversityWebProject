using Logger;
using ServiceWorkerCronJobDemo.Services;
using UJob;

namespace Start;

public class InfoJob(
    IScheduleConfig<TeacherDoWorkJob> config,
    ILogger<TeacherDoWorkJob> loggerMain,
    MyLogger myLogger,
    IPrintWorkersJob printWorkersJob,
    IPrintStudentJob printStudentJob,
    IScoresOfStudentsJob scoresOfStudentsJob,
    IInfoCouplesAttendanceJob infoCouplesAttendanceJob)
    : CronJobService(config.CronExpression, config.TimeZoneInfo, loggerMain)
{
    public override Task StartAsync(CancellationToken cancellationToken)
    {
        loggerMain.LogInformation("CronJob запущен");
        myLogger.Info("CronJob запущен", "Start:StartJob:InfoJob");
        return base.StartAsync(cancellationToken);
    }

    public override Task DoWork(CancellationToken cancellationToken)
    {
        loggerMain.LogInformation($"{DateTime.Now:hh:mm:ss} Выполняется задача");
        myLogger.Info($"{DateTime.Now:hh:mm:ss} Выполняется задача", "Start:StartJob:InfoJob"); 
        Console.WriteLine("Вывод интересующей вас инфо. Если о рабочих, введите 1, если о студентах, введите 2. Если о баллах студентов - 3, если о пропусках студентов - 4");
        int input = int.Parse(Console.ReadLine()??"0");
        switch (input)
        {
            case 1:
                printWorkersJob.DoWorkAsync();
                break;
            case 2:
                printStudentJob.DoWorkAsync();
                break;
            case 3:
                scoresOfStudentsJob.DoWorkAsync();
                break;
            case 4:
                infoCouplesAttendanceJob.DoWorkAsync();
                break;
            default:
                myLogger.Info("Выход за возможный выбор", "Start:StartJob:InfoJob");
                Console.WriteLine("Повторите ввод");
                break;
        }
        return Task.CompletedTask;
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        loggerMain.LogInformation("CronJob остановлен");
        myLogger.Info("CronJob остановлен", "Start:StartJob:InfoJob");
        return base.StopAsync(cancellationToken);
    }
}