using Microsoft.AspNetCore.Mvc;

namespace TimesheetSystem.Controllers
{
    public class TimesheetController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult ProjectTotals()
        {
            return View();
        }
        public IActionResult UserWeek()
        {
            return View();
        }
    }
}
