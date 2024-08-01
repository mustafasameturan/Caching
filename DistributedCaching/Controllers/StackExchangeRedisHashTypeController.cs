using DistributedCaching.Services;
using Microsoft.AspNetCore.Mvc;
using StackExchange.Redis;

namespace DistributedCaching.Controllers;

[ApiController]
[Route("api/[controller]/[action]")]
public class StackExchangeRedisHashTypeController : ControllerBase
{
    private readonly IDatabase db;
    private string hashKey { get; set; } = "dict";

    public StackExchangeRedisHashTypeController(RedisService redisService)
    {
        db = redisService.GetDb(4);
    }

    [HttpPost]
    public async Task<IActionResult> CacheHashData(string key, string value)
    {
        await db.HashSetAsync(hashKey, key, value);
        return Ok("added");
    }
    
    [HttpGet]
    public IActionResult GetCachedHashes()
    {
        Dictionary<string, string> dicts = new Dictionary<string, string>();

        if (!db.KeyExists(hashKey)) return NotFound("data not found!");

        db.HashGetAll(hashKey)
            .ToList()
            .ForEach(x =>
            {
                dicts.Add(x.Name.ToString(), x.Value.ToString());
            });

        return Ok(dicts);
    }

    [HttpGet]
    public async Task<IActionResult> GetCachedHashByKey(string key)
    {
        var a =await db.HashGetAsync(hashKey, key);
        return Ok(a.ToString());
    }
    

    [HttpDelete]
    public async Task<IActionResult> RemoveNameFromCachedHashes(string key)
    {
        await db.HashDeleteAsync(hashKey, key);
        return Ok("deleted");
    }
}