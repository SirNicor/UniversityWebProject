namespace IRepositoryAll;
using UCore;
using Logger;

public interface IScheduleRepository
{
    public long Create(ScheduleDto schedule);
    public Schedule Get(long id);
    public List<Schedule> ReturnList();
    public List<Schedule> ReturnListForDirectionId(long dirId);
    public void Delete(long id);
    public long Update(ScheduleDto schedule);
}