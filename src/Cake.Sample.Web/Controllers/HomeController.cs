using Cake.Sample.Lib;
using System.Web.Mvc;

namespace Cake.Sample.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly SuperClass super;

        public HomeController()
        {
            this.super = new SuperClass(1);
        }

        public ActionResult Index()
        {
            return View((object)this.super.DoSuperStuff());
        }
    }
}