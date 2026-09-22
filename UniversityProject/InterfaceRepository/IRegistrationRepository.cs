using UCore;

namespace IRepositoryAll;

public interface IRegistrationRepository
{
    public RegistrationDTO Get(string login);
    public long Create(RegistrationDTO registration);
    public long Update(RegistrationDTO registration);
}