using Microsoft.AspNetCore.Mvc;

namespace ReadService1
{
    [Route("api")]
    public class ReadController : Controller
    {
        [HttpGet("helloWorld")]
        public ActionResult Hello()
        {
            return Ok("Hello");
        }
    }
}