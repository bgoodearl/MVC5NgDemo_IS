using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using MVCDemo.ViewModels.Music;
using BGoodMusic.EFDAL.Interfaces;

namespace MVCDemo.Controllers
{
    [RoutePrefix("demo/music")]
    [Route("{action}")]
    public class MusicController : Controller
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

        public ActionResult Rehearsals()
        {
            return View();
        }
    }
}