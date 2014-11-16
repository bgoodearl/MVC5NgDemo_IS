using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SSC = System.Security.Claims;
using MVCDemo.ViewModels.Test;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MVCDemo.ViewModels.Shared;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using TIMC = Thinktecture.IdentityModel.Client;

namespace MVCDemo.Controllers
{
    [RoutePrefix("demo/test")]
    [Route("{action}")]
    public class TestController : DemoControllerBase
    {

        [AllowAnonymous]
        public ActionResult Index()
        {
            return View();
        }

        public async Task<ActionResult> AngularTest()
        {
            AngularTest model = new ViewModels.Test.AngularTest();

            StringBuilder msg = new StringBuilder();
            try
            {
                string token = GetRefreshTokenFromClaim(this.User as SSC.ClaimsPrincipal);
                var tokenClient = new TIMC.OAuth2Client(
                    new Uri(MVCDemo.Startup.Config.TokenEndpoint), //"https://localhost:44311/identity/connect/token"),
                    MVCDemo.Startup.Config.ClientId,
                    MVCDemo.Startup.Config.ClientSecret);

                var response = await tokenClient.RequestRefreshTokenAsync(token);
                if (response.IsError)
                {
                    msg.AppendFormat("Token Client Error: {1}: {0}", Environment.NewLine, response.Error);
                }
                else if (response.AccessToken != null)
                {
                    msg.AppendFormat("Access Token: \"{1}\"{0}Refresh Token: \"{2}\"{0}",
                        Environment.NewLine,
                        response.AccessToken,
                        response.RefreshToken
                        );
                    if (response.Json != null)
                    {
                        msg.AppendFormat("Json: \"{1}\"{0}",
                            Environment.NewLine,
                            response.Json);
                    }
                    TokenInfo ti = new TokenInfo
                    {
                        Tk = response.AccessToken
                    };
                    model.JsonToken = JsonConvert.SerializeObject(ti,
                        new JsonSerializerSettings
                        {
                            ContractResolver = new CamelCasePropertyNamesContractResolver()
                        });
                }
            }
            catch (Exception ex)
            {
                ReportException(ex, msg);
            }
            ViewBag.Message = msg.ToString();

            return View(model);
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