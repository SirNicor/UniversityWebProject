namespace IRepositoryAll;
using UCore;

public interface IAuthorizationRepository
{
    public Task<AuthorizationDto> GetForLoginAuthorizationAsync(string login);
    public Task<AuthorizationDto> GetForIdAuthorizationAsync(long id);
    public Task<long> CreateAuthorizationAsync(AuthorizationDto dto);
    public Task<long> UpdateAuthorizationAsync(AuthorizationDto dto);
    public Task<(long?, int[]?)> GetAuthorizationsRoleForIndexAsync(AuthorizationForGetJwtToken dto);
    public Task<string[]> GetAllRolesAsync(int[] idRoles);
    public Task<bool> CheckPasswordAsync(string password, long id);
    public Task<RefreshJWTTokenDTO> GetJwtTokenAsync(string token);
    public Task<long?> CheckAndUpdateJwtTokenAsync(string token);
    public Task<long> CreateJwtTokenAsync(RefreshJWTTokenDTO dto);
    public Task DeleteJwtTokenAsync(string token);
}