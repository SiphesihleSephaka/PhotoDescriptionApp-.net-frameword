using Microsoft.AspNetCore.Mvc;

public class ResponseCachingController : Controller
{
    [HttpGet]
    [ResponseCache(Duration = 60, Location = ResponseCacheLocation.Client)]
    public IActionResult GetResponseCachedData()
    {
        return Ok("This is the response cached data.");
    }
}
