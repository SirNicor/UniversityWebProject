namespace IRepositoryAll;
using UCore;
using Logger;

public interface IWorkerAdministratorRepository
{
    public void PrintAll();
    public long Create(Administrator administrator);
    public List<Administrator> ReturnListAdministrator();
    Administrator Get(long id);
    public void Delete(long id);
    public long Update(Administrator idAndAdministrator);
}