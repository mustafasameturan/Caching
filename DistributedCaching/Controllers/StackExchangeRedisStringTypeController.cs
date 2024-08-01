using DistributedCaching.Services;
using Microsoft.AspNetCore.Mvc;
using StackExchange.Redis;

namespace DistributedCaching.Controllers;

[ApiController]
[Route("api/[controller]/[action]")]
public class StackExchangeRedisStringTypeController : ControllerBase
{
    private readonly IDatabase db;

    public StackExchangeRedisStringTypeController(RedisService redisService)
    {
        db = redisService.GetDb(0);
    }

    [HttpPost]
    public IActionResult CacheStringData()
    {
        db.StringSet("name", "Mustafa Samet");
        db.StringSet("visitors", 1000);

        return Ok();
    }

    [HttpPost]
    public async Task<IActionResult> IncreaseVisitorsData(int count)
    {
        await db.StringIncrementAsync("visitors", count);
        return Ok();
    }
    
    [HttpPost]
    public async Task<IActionResult> DecrementVisitorsData(int count)
    {
        await db.StringDecrementAsync("visitors", count);
        return Ok();
    }

    [HttpGet]
    public IActionResult GetCachedStringData()
    {
        var firstValue = db.StringGet("name");
        var secondValue = db.StringGet("visitors");
        var firstValueLength = db.StringLength("name");
        var firstValueRange = db.StringGetRange("name", 0, 3);

        string[] values =
        {
            firstValue.ToString(), 
            secondValue.ToString(), 
            firstValueLength.ToString(), 
            firstValueRange.ToString()
        };

        return Ok(values);
    }
}