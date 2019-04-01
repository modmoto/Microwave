using Microsoft.AspNetCore.Mvc;

namespace ReadService1
{
    [Route("api")]
    public class ReadController : Controller
    {
        [HttpGet("hello")]
        public ActionResult Hello()
        {
            return Ok("Hello");
        }
    }
}