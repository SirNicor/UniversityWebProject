namespace IRepositoryAll;
using UCore;
using Logger;

public interface IWorkerTeacherRepository
{
    public void PrintAll();
    public long Create(Teacher teacher);
    public List<Teacher> ReturnList();
    List<Teacher> GetForIds(List<long> ids);
    public Teacher GetForId(long id);
    public void Delete(long id);
    public long Update(Teacher teacher);
    
}