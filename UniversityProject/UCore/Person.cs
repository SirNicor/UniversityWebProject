    using System.Diagnostics.CodeAnalysis;

    namespace UCore;
    using Logger;
    public abstract class Person
    {
        public void PrintInfo(MyLogger myLogger)
        {
            string message = $"";
            myLogger.Info(message, "UCore:Person");
            Passport.Print(myLogger);
            message = ($"Военный билет: {Millitary.LevelId} и судимость ") + (CriminalRecord?"есть":"нет");
            myLogger.Info(message, "UCore:Person");
            PrintDerivedClass(myLogger);
        }
        public abstract void PrintDerivedClass(MyLogger myLogger);
        public long Id { get; set; }
        public long PassportId { get; set; }
        public int MillitaryId { get; set; }
        public MillitaryClass Millitary { get; set; }
        public Passport Passport { get; set; }
        public bool CriminalRecord { get; set; }
    }