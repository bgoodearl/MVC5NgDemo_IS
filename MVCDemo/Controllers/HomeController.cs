using System.Web.Mvc;

namespace MVCDemo.Controllers
{
    [RoutePrefix("demo/home")]
    [Route("{action}")]
    public class HomeController : Controller
    {
        [AllowAnonymous]
        [Route("~/demo/")]
        [Route]
        [Route("index")]
        public ActionResult Index()
        {
            ViewBag.Title = "Home Page";

            return View();
        }
    }
}
