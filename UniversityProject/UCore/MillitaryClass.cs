namespace UCore;

public class MillitaryClass
{
    public int MillitaryId { get; set; }
    public string LevelId { get; set; }
    public ICollection<Student> Student { get; set; } = new List<Student>();
}