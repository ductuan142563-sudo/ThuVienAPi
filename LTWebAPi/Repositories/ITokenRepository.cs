using Microsoft.AspNetCore.Identity;

namespace LTWebAPI.Repositories
{
    public interface ITokenRepository
    {
        string CreateJWTToken(IdentityUser user, List<string> roles);
    }
}