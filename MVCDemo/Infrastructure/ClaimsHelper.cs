using System.Linq;
using System.Security.Claims;

namespace MVCDemo.Infrastructure
{
    public static class ClaimsHelper
    {
        public static Claim GetClaim(this ClaimsPrincipal principal, string claimType)
        {
            if ((principal != null) && (principal.Claims != null) && !string.IsNullOrWhiteSpace(claimType))
            {
                var claim = principal.Claims.Where(c => c.Type == claimType).FirstOrDefault();
                return claim;
            }
            return null;
        }

        public static string GetNameIdentiferValue(this ClaimsPrincipal principal)
        {
            var claim = GetClaim(principal, ClaimTypes.NameIdentifier);
            if (claim != null)
                return claim.Value;
            return null;
        }
    }
}