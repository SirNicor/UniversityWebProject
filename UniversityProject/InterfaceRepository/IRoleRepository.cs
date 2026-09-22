namespace IRepositoryAll;
using UCore;

public interface IRoleRepository
{
    public RoleAccessDto GetRoleAccess(int roleId);
    public RoleAccessDto[] GetRoleAccess(int[] rolesId);
}