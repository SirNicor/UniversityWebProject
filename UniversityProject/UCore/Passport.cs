using System.Security.Cryptography;

namespace UCore;
using Logger;
public class Passport
{
    public void Print(MyLogger myLogger)
    {
        string message = $"Id: {PassportId}, FullName: {FirstName} {LastName} {MiddleName}, BirthDate: {BirthData}";
        message += Environment.NewLine + $"Serial: {Serial} Number: {Number} issued by whom: {PlaceReceipt}";
        myLogger.Info(message, "UCore:Passport");
        Address.Print(myLogger);
    }
    public long PassportId { get; set; }
    public string Serial { get; set; }
    public string Number { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string MiddleName { get; set; }
    public DateOnly BirthData { get; set; }
    public Address Address { get; set; }
    public long? AddressId { get; set; } = null;
    public string PlaceReceipt { get; set; }
    public Student student { get; set; } = null;
}