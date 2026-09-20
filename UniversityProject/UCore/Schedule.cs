namespace UCore;

public class Schedule
{
    public int Id { get; set; }
    public Direction Direction { get; set; }
    public Discipline Discipline { get; set; }
    public Teacher Teacher { get; set; }
    public string DataWeek { get; set; }
    public string  StartCouple { get; set; }
    public string EndCouple { get; set; }
}