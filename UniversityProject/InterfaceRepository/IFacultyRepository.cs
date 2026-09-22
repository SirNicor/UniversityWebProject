namespace IRepositoryAll;
using UCore;
using Logger;

public interface IFacultyRepository
{
    public long Create(FacultyDto facutlyDto);
    public Faculty Get(long id);
    public List<Faculty> ReturnList();
    public void Delete(long id);
    public long Update(FacultyDto faculty);
    public long? CheckNameFaculty(string nameFaculty, long universityId);
}