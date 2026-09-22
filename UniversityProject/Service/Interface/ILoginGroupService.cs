using UCore;
using UCore.DTO;

namespace Service;
using Ucore;

public interface ILoginGroupService
{
    public Task<JWTTokenDto?> AsyncLogin(AuthorizationForGetJwtToken auth, CancellationToken token);
    public Task<JWTTokenDto> AsyncResetAccessToken(string jwt);
}