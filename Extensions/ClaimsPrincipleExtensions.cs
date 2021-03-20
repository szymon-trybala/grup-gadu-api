using System.Security.Claims;

namespace grup_gadu_api.Extensions
{
     public static class ClaimsPrincipleExtensions
    {
        public static string GetLogin(this ClaimsPrincipal user)
        {
            return user.FindFirst(ClaimTypes.Name)?.Value;
        }

        public static int GetUserId(this ClaimsPrincipal user)
        {
            return int.Parse(user.FindFirst(ClaimTypes.NameIdentifier)?.Value);
        }
    }
}