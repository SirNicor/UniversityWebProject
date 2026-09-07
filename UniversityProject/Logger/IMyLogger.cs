namespace Logger;

public interface IMyLogger
{
    public void Debug(string message, string typeMethod);
    public void Debug(string message, string typeMethod, Exception exception);
    public void Info(string message, string typeMethod);
    public void Info(string message, string typeMethod, Exception exception);
    public void Warning(string message, string typeMethod);
    public void Warning(string message, string typeMethod, Exception exception);
    public void Error(string message, string typeMethod);
    public void Error(string message, string typeMethod, Exception exception);
    public void Fatal(string message, string typeMethod);
    public void Fatal(string message, string typeMethod, Exception exception);
}