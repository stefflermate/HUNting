using HUNting.API.Models.Users;

namespace HUNting.API.Services.Auth
{
    public record TokenResult(string AccessToken, DateTime ExpiresAt);

    public interface ITokenService
    {
        TokenResult CreateToken(ApplicationUser user, IList<string> roles);
    }
}
