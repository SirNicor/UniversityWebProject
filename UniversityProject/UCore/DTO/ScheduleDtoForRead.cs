namespace UCore.DTO;

public class ScheduleDtoForRead
{
    public int Id { get; set; }
    public long DirectionId { get; set; }
    public long DisciplineId { get; set; }
    public long TeacherId { get; set; }
    public string DataWeek { get; set; }
    public string  StartCouple { get; set; }
    public string EndCouple { get; set; }
}