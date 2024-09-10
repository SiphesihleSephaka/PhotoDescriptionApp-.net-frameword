using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using System;

public class CachingController : Controller
{
    private readonly IMemoryCache _cache;

    public CachingController(IMemoryCache cache)
    {
        _cache = cache;
    }

    public IActionResult Index()
    {
        string cacheKey = "cachedData";
        if (!_cache.TryGetValue(cacheKey, out string cachedData))
        {
            // Simulate data retrieval from a database or external service
            cachedData = "This is the cached data.";

            var cacheEntryOptions = new MemoryCacheEntryOptions()
                .SetSlidingExpiration(TimeSpan.FromMinutes(5))
                .SetAbsoluteExpiration(TimeSpan.FromHours(1));

            _cache.Set(cacheKey, cachedData, cacheEntryOptions);
        }

        // Pass the cached data to the view
        return View((object)cachedData);
    }
}
