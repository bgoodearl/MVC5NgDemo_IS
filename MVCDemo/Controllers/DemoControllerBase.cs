using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SSC = System.Security.Claims;
using System.Text;
using System.Threading;
using MVCDemo.Infrastructure;
using BGoodMusic.EFDAL.Interfaces;

namespace MVCDemo.Controllers
{
    public class DemoControllerBase : Controller
    {
        protected string GetRefreshTokenFromClaim(SSC.ClaimsPrincipal principal)
        {
            if (principal == null)
                throw new InvalidOperationException("Claims Principal not specified");
            var claim = principal.GetClaim(Constants.LocalClaimTypes.RefreshToken);
            if (claim != null)
                return claim.Value;
            return null;
        }

        protected StringBuilder ReportException(Exception ex, StringBuilder sb)
        {
            sb.AppendFormat("{1}: {2}{0}", Environment.NewLine, ex.GetType().Name, ex.Message);
            if (ex.InnerException != null)
            {
                Exception ex2 = ex.InnerException;
                sb.AppendFormat("{1}: {2}{0}",
                    Environment.NewLine, ex2.GetType().Name, ex2.Message);
                if (ex2.InnerException != null)
                {
                    ex2 = ex2.InnerException;
                    sb.AppendFormat("{1}: {2}{0}",
                        Environment.NewLine, ex2.GetType().Name, ex2.Message);
                }
            }
            return sb;
        }

        protected StringBuilder ReportException(Exception ex)
        {
            StringBuilder sb = new StringBuilder();
            return ReportException(ex, sb);
        }

    }
}