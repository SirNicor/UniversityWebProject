using System.IdentityModel.Tokens.Jwt;

namespace UCore.DTO;

public class JWTTokenDto
{
    public JwtSecurityToken AccessToken { get; set; }
    public JwtSecurityToken RefreshToken { get; set; }
    public string[]? Roles { get; set; }
    public string? Message { get; set; }
    public int? HttpCode { get; set; }
}