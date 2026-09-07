namespace UCore;
using Logger;

public class Address
{
    public Address(string country, string city, string street, string houseNumber)
    {
        Country = country;
        Street = street;
        City = city;
        HouseNumber = houseNumber;
    }
    public Address(){}
    public void Print(MyLogger myLogger)
    {
        if (AddressString == "")
        {
            myLogger.Info($"Id: {AddressId}, FullAddress: {AddressString}", "UCore:Address");
        }
        else
        {
            myLogger.Info($"Id: {AddressId}, FullAddress: " + Country + " " + City + " " + Street + " " + HouseNumber, "UCore:Address");
        }
    }
    public long AddressId { get; set; }
    public string Country { get; set; }
    public string City { get; set; }
    public string Street { get; set; }
    public string HouseNumber { get; set; }
    public string AddressString { get; set; } = "";
    public ICollection<Passport> Passports { get; set; } = new List<Passport>();
}