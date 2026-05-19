namespace Homecare.Application.Interfaces.Auth;
public interface ITokensService
{
    string GenerateAccessToken(int id, string email, string role);
    string GenerateRefreshToken();
}
