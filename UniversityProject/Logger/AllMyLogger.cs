namespace Logger;

public class AllMyLogger(MyLogger[] loggers) : MyLogger
{
    protected override void Log(LevelLoger levelLoger, string message, string typeMethod)
    {
        Log(levelLoger, message, typeMethod, null);
    }

    protected override void Log(LevelLoger levelLoger, string message, string typeMethod, Exception exception)
    {
        foreach (var loggers1 in loggers)
        {
            switch (levelLoger)
            {
                case LevelLoger.DEBUG:
                    loggers1.Debug(message, typeMethod);
                    break;
                case LevelLoger.INFO:
                    loggers1.Info(message, typeMethod);
                    break;
                case LevelLoger.WARNING:
                    loggers1.Warning(message, typeMethod);
                    break;
                case LevelLoger.ERROR:
                    loggers1.Error(message, typeMethod);
                    break;
                case LevelLoger.FATAL:
                    loggers1.Fatal(message, typeMethod);
                    break;
            }
        }
    }
}   