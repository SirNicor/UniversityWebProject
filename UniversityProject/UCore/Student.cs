using System.ComponentModel.DataAnnotations;
using System.Net.Security;
using Logger;

namespace UCore;
using Logger;
public class Student
{
    public long PassportId { get; set; }
    public int MillitaryId { get; set; }
    public MillitaryClass Millitary { get; set; }
    public Passport Passport { get; set; }
    public bool CriminalRecord { get; set; }
    protected const int MinChances = 200;
    public long StudentId { get; set; }
    public double? TotalScore { get; set; }
    public long? SkipHours { get; set; }
    public long? CreditScores { get; set; }
    public long? CountOfExamsPassed { get; set; }
    public long? Course { get; set; } 

    public string? ChatId { get; set; }
    public void SkipHoursSet(int skipHours)
    {
        if (skipHours == null)
        {
            SkipHours = 0;
        }
        else if (skipHours < 0)
        {
            SkipHours += 0;
        }
        else
        {
            SkipHours += skipHours*2;
        }
    }

    public void CreditScoresSet(int creditScores)
    {
        if (creditScores == null)
        {
            CreditScores = 0;
        }
        else if (creditScores < 0)
        {
            CreditScores -= 1;
        }
        else
        {
            CreditScores += creditScores ;
        }
    }
    
    public void NextExamsPassed()
    {
        CountOfExamsPassed++;
    }
    public void PrintDerivedClass(MyLogger myLogger)
    {
        string message = $"";
        Passport.Print(myLogger);
        message = ($"Военный билет: {Millitary.LevelId} и судимость ") + (CriminalRecord?"есть":"нет");
        myLogger.Info(message);
        message = $"Course: {Course}" + Environment.NewLine;
        message += $"Общий балл ={CreditScores} и количество сданных экзаменов = {CountOfExamsPassed} и общий балл = {TotalScore}" + Environment.NewLine;
        // message += "Расположен ли в общежитии " + (_accomodationDormitories ? "Да" : "Нет");
        myLogger.Info(message);
    }

    // private bool _accomodationDormitories;

}



