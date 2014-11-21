using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using SSC = System.Security.Claims;
using MVCDemo.ViewModels.Music;
using BGoodMusic.EFDAL.Interfaces;
using BGoodMusic.Models;
using MVCDemo.Infrastructure;
using MVCDemo.ViewModels.Shared;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using TIMC = Thinktecture.IdentityModel.Client;

namespace MVCDemo.Controllers
{
    [RoutePrefix("demo/music")]
    [Route("{action}")]
    public class MusicController : DemoControllerBase
    {
        // GET: Music
        [Route]
        [Route("index")]
        public ActionResult Index()
        {
            IBGoodMusicRepository repo = new BGoodMusic.EFDAL.BGoodMusicDBContext();
            List<RehearsalListItem> itemList = new List<RehearsalListItem>();
            foreach(var r in repo.GetRehearsals().ToList())
            {
                itemList.Add(new RehearsalListItem
                    {
                        Id = r.Id,
                        Date = r.Date,
                        Duration = r.Duration,
                        Location = r.Location,
                        Time = r.Time
                    });
            }
            return View(itemList);
        }

        // Left as an exercise to the reader
        // Move common threaded code into ControllerBase
        // (See same code in Tests controller AngularTest method)
        [Route("rehearsals")]
        public async Task<ActionResult> Rehearsals()
        {
            StringBuilder msg = new StringBuilder();
            RehearsalViewModel model = new RehearsalViewModel();
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
                    msg.AppendFormat("Access Token (len={1}): \"{2}...{3}\"{0}",
                        Environment.NewLine,
                        response.AccessToken.Length,
                        response.AccessToken.Substring(0, 16),
                        response.AccessToken.Substring(response.AccessToken.Length-16));
                    //Uncomment for more information when testing/exploring the app
                    //msg.AppendFormat("Access Token: \"{1}\"{0}Refresh Token: \"{2}\"{0}",
                    //    Environment.NewLine,
                    //    response.AccessToken,
                    //    response.RefreshToken
                    //    );
                    //if (response.Json != null)
                    //{
                    //    msg.AppendFormat("Json: \"{1}\"{0}",
                    //        Environment.NewLine,
                    //        response.Json);
                    //}
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
    }
}