using grup_gadu_api.Entities;

namespace grup_gadu_api.Interfaces
{
    public interface ITokenService
    {
         string CreateToken(AppUser user);
    }
}