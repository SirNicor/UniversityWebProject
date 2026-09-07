namespace UCore;
using Logger;
public class Administrator : Worker
{
    public override void PrintDerivedClass(MyLogger myLogger)
    {
        string message;
        message = $"Зарплата - {Salary}" + Environment.NewLine;
        myLogger.Info(message, "UCore:Administrator");
    }

    public override void DoWork(MyLogger myLogger)
    {
        throw new NotImplementedException();
    }
    
    public string Post { get; set; }
    public long AdministratorId { get; set; }
}