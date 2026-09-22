namespace IRepositoryAll;
using UCore;
using Logger;

public interface IUniversityRepository
{
    public long Create(UniversityDto university);
    public University Get(long id);
    public long? CheckNameInUniversity(string nameUniversity);
    public List<University> ReturnList();
    public void Delete(long id);
    public long Update(UniversityDto university);
}