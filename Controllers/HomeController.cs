using Microsoft.AspNetCore.Mvc;

namespace ANPR.Controllers
{
    public class HomeController : Controller
    {
        /// <summary>
        /// First Methods to call which respond in starting the app
        /// </summary>
        /// <returns></returns>
        public IActionResult Index() => Ok("The project started successfully");

        /// <summary>
        /// This Methods only run if there is any error
        /// </summary>
        /// <returns></returns>
        public IActionResult Error() =>
            BadRequest("There's problem occure while trying to excute your process.\nPlease try again.");
    }
}