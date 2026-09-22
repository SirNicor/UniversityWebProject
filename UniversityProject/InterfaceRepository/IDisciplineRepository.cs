namespace IRepositoryAll;
using UCore;
using Logger;


public interface IDisciplineRepository
{
    public long Create(DisciplineDto discipline);
    public List<Discipline> GetForIds(List<long> ids);
    public Discipline GetForId(long id);
    public List<Discipline> ReturnList();
    public void Delete(long id);
    public long Update(DisciplineDto discipline);
}