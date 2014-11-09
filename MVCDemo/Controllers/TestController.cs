using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SSC = System.Security.Claims;
using MVCDemo.ViewModels.Test;
using System.Text;

namespace MVCDemo.Controllers
{
    [RoutePrefix("demo/test")]
    [Route("{action}")]
    public class TestController : Controller
    {

        [AllowAnonymous]
        public ActionResult Index()
        {
            return View();
        }

        // GET: Test/ClientInfo
        [AllowAnonymous]
        public ActionResult ClientInfo()
        {
            StringBuilder msg = new StringBuilder();
            if (!Request.IsAuthenticated)
            {
                msg.AppendLine("Request is not authenticated.");
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(User.Identity.Name))
                    msg.AppendFormat("Identity Name = \"{1}\"{0}", Environment.NewLine, User.Identity.Name);
                else
                    msg.AppendLine("Identity Name is null or empty.");
            }
            ClientInfoViewModel model = new ClientInfoViewModel();
            var user = this.User;
            var claimsPrincipal = user as SSC.ClaimsPrincipal;
            if (claimsPrincipal != null)
            {
                SSC.ClaimsIdentity claimsIdentity = claimsPrincipal.Identity as SSC.ClaimsIdentity;
                if (claimsIdentity != null)
                {
                    int claimNum = 1;
                    foreach (SSC.Claim claim in claimsIdentity.Claims)
                    {
                        string claimValue = claim.Value;
                        if (!string.IsNullOrWhiteSpace(claimValue) && claimValue.Length > 100)
                        {
                            int lastSpaceIdx = claimValue.LastIndexOf(' ');
                            if (lastSpaceIdx < 0)
                                claimValue = claimValue.Substring(0, 95) + "...";
                        }
                        model.ClientInfo.Add(new InfoItem
                        {
                            Name = string.Format("Claim {0}", claimNum),
                            Value = string.Format("Issuer: \"{1}\" {0}OriginalIssuer: \"{2}\" {0}Properties.Count: \"{3}\" {0}Subject: \"{4}\" {0}Type: \"{5}\" {0}ValueType: \"{6}\" {0}Value: \"{7}\" {0}",
                                    Environment.NewLine,
                                    claim.Issuer,
                                    claim.OriginalIssuer,
                                    claim.Properties.Count,
                                    claim.Subject,
                                    claim.Type,
                                    claim.ValueType,
                                    claimValue)
                        });
                        claimNum++;
                    }
                }
            }
            ViewBag.Message = msg.ToString();
            return View(model);
        }
    }
}