namespace Logger;

public class FileMyLogger : MyLogger
{
    public FileMyLogger()
    {
        lock (locker)
        {
            string currentTime = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
            string workingDir = Environment.CurrentDirectory;
            _fileName = Path.Combine(workingDir, "Logs");
            if (!Directory.Exists(_fileName))
            {
                Directory.CreateDirectory(_fileName);
            }
            _fileName = Path.Combine(_fileName, $"log_{currentTime}.txt");
            File.AppendAllText(_fileName, DateTime.Now + ": создан новый лог файл" + Environment.NewLine);
        }
    }
    protected override void Log(LevelLoger levelLoger, string message, string typeMethod)
    {
        Log(levelLoger, message, typeMethod, null);
    }

    protected override void Log(LevelLoger levelLoger, string message, string typeMethod, Exception exception)
    {
        lock (locker)
        {
            
            if (levelLoger < MinLog)
                return;
            CurrentTime = DateTime.Now;
            var logMessage = $"{CurrentTime}, {typeMethod}: {levelLoger}: {message}";
            if (exception != null)
            {
                logMessage += Environment.NewLine + exception.StackTrace;
            }
            File.AppendAllText(_fileName, logMessage + Environment.NewLine);
        }
    }
    private readonly string _fileName;
    private Lock locker = new();
}