using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace MVCDemo.Controllers
{
    [RequireHttps]
    [RoutePrefix("demo/auth")]
    [Route("{action}")]
    public class AuthController : Controller
    {
        // GET: Auth
        [AllowAnonymous]
        public ActionResult Login()
        {
            return RedirectToAction("ForceLogin");
        }

        [AllowAnonymous]
        public ActionResult LoginFailed()
        {
            return View();
        }

        public ActionResult ForceLogin()
        {
            return View();
        }

        [AllowAnonymous]
        public ActionResult AuthResponse(string error, string state)
        {
            StringBuilder msg = new StringBuilder();
            if (Request.IsAuthenticated)
            {
                msg.Append("Is Authenticated!");
            }
            else
            {
                msg.AppendLine("Not Authenticated :-(");
            }
            if (!string.IsNullOrWhiteSpace(error))
                msg.AppendFormat("Error: {1}{0}", Environment.NewLine, error);
            if (!string.IsNullOrWhiteSpace(state))
                msg.AppendFormat("State: {1}{0}", Environment.NewLine, state);
            ViewBag.Message = msg.ToString();
            return View();
        }

        [AllowAnonymous]
        public ActionResult LoggedOut()
        {
            return View();
        }

        public void Logout()
        {
            var ctx = Request.GetOwinContext();
            ctx.Authentication.SignOut();
        }

    }
}