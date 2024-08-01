using DistributedCaching.Services;
using Microsoft.AspNetCore.Mvc;
using StackExchange.Redis;

namespace DistributedCaching.Controllers;

[ApiController]
[Route("api/[controller]/[action]")]
public class StackExchangeRedisListTypeController : ControllerBase
{
    private readonly IDatabase db;
    private readonly string listKey = "names";

    public StackExchangeRedisListTypeController(RedisService redisService)
    {
        db = redisService.GetDb(4);
    }
    
    [HttpPost]
    public async Task<IActionResult> CacheListData(string name, string direction)
    {
        switch (direction)
        {
            case "left":
                await db.ListLeftPushAsync(listKey, name);
                break;
            case "right":
                await db.ListRightPushAsync(listKey, name);
                break;
        }

        return Ok("added");
    }

    [HttpGet]
    public IActionResult GetCachedNames(int start = 0, int stop = -1)
    {
        List<string> namesList = new List<string>();

        if (!db.KeyExists(listKey)) return BadRequest("key not found!");

        db.ListRange(listKey, start, stop)
            .ToList()
            .ForEach(x =>
            {
                namesList.Add(x.ToString());
            });

        return Ok(namesList);
    }

    [HttpDelete]
    public async Task<IActionResult> RemoveNameFromCachedNamesList(string name)
    {
        await db.ListRemoveAsync(listKey, name);
        return Ok("deleted");
    }

    [HttpDelete]
    public async Task<IActionResult> DeleteLeftOrRight(string direction)
    {
        switch (direction)
        {
            case "left":
                await db.ListLeftPopAsync(listKey);
                break;
            case "right":
                await db.ListRightPopAsync(listKey);
                break;
        }

        return Ok($"deleted from {direction}");
    }
}