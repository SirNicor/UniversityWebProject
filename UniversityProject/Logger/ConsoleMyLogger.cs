namespace Logger;

public class ConsoleMyLogger : MyLogger
{
    protected override void Log(LevelLoger levelLoger, string message, string typeMethod)
    {
        Log(levelLoger, message, typeMethod, null);
    }
    protected override void Log(LevelLoger levelLoger, string message, string typeMethod, Exception? exception)
    {
        if (levelLoger < MinLog)
            return;
        CurrentTime = DateTime.Now;
        var logMessage = $"{CurrentTime}, {typeMethod}: {levelLoger}: {message}";
        if (exception != null)
        {
            logMessage += Environment.NewLine + exception.StackTrace;
        }
        Console.WriteLine(logMessage);
    }
}