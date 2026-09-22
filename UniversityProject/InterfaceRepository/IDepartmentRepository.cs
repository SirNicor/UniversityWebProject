using Logger;

namespace IRepositoryAll;

using UCore;
using static MyLogger;

public interface IDepartmentRepository
{
    public long Create(DepartmentDto department);
    public Department Get(long id);
    public List<Department> ReturnList();
    public void Delete(long id);
    public long Update(DepartmentDto department);
    public long? CheckNameDepartment(string nameDepartment, long facultyId);
}
