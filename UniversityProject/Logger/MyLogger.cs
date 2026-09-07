namespace Logger;

public abstract class MyLogger:IMyLogger
{
    public LevelLoger MinLog { get; set; }      
    public bool ExplainLog { get; set; }
    protected DateTime CurrentTime;
    protected abstract void Log(LevelLoger levelLoger,  string message, string typeMethod);
    protected abstract void Log(LevelLoger levelLoger, string message, string typeMethod, Exception exception);

    public void Debug(string message, string typeMethod)
    {
        Log(LevelLoger.DEBUG, message, typeMethod);
    }

    public void Debug(string message, string typeMethod, Exception exception)
    {
        Log(LevelLoger.DEBUG, message, typeMethod, exception);
    }

    public void Info(string message, string typeMethod)
    {
        Log(LevelLoger.INFO, message, typeMethod);
    }

    public void Info(string message, string typeMethod, Exception exception)
    {
        Log(LevelLoger.INFO, message, typeMethod, exception);
    }

    public void Warning(string message, string typeMethod)
    {
        Log(LevelLoger.WARNING, message, typeMethod);
    }

    public void Warning(string message, string typeMethod, Exception exception)
    {
        Log(LevelLoger.WARNING, message, typeMethod, exception);
    }

    public void Error(string message, string typeMethod)
    {
        Log(LevelLoger.ERROR, message, typeMethod);
    }

    public void Error(string message, string typeMethod, Exception exception)
    {
        Log(LevelLoger.ERROR, message, typeMethod, exception);
    }

    public void Fatal(string message, string typeMethod)
    {
        Log(LevelLoger.FATAL, message, typeMethod);
    }

    public void Fatal(string message, string typeMethod, Exception exception)
    {
        Log(LevelLoger.FATAL, message, typeMethod, exception);
    }
}